using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using Chilkat;
using TcpDotNet.Protocol;
using Stream = System.IO.Stream;
using Task = System.Threading.Tasks.Task;

namespace TcpDotNet;

/// <summary>
///     Represents a client node.
/// </summary>
public abstract class ClientNode : Node
{
    private readonly ObjectIDGenerator _callbackIdGenerator = new();
    private readonly ConcurrentDictionary<int, List<TaskCompletionSource<Packet>>> _packetCompletionSources = new();
    private EndPoint? _remoteEP;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientNode" /> class.
    /// </summary>
    protected ClientNode()
    {
    }

    /// <summary>
    ///     Gets a value indicating whether the client is connected.
    /// </summary>
    /// <value><see langword="true" /> if the client is connected; otherwise, <see langword="false" />.</value>
    public bool IsConnected { get; protected set; }

    /// <summary>
    ///     Gets the remote endpoint.
    /// </summary>
    /// <value>The <see cref="EndPoint" /> with which the client is communicating.</value>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public EndPoint RemoteEndPoint
    {
        get => _remoteEP ??= BaseSocket.RemoteEndPoint;
        internal set => _remoteEP = value;
    }

    /// <summary>
    ///     Gets the session ID of the client.
    /// </summary>
    /// <value>The session ID.</value>
    public Guid SessionId { get; internal set; }

    /// <summary>
    ///     Gets the current state of the client.
    /// </summary>
    public ClientState State { get; protected internal set; }

    /// <summary>
    ///     Gets or sets a value indicating whether GZip compression is enabled.
    /// </summary>
    /// <value><see langword="true" /> if compression is enabled; otherwise, <see langword="false" />.</value>
    internal bool UseCompression { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether encryption is enabled.
    /// </summary>
    /// <value><see langword="true" /> if encryption is enabled; otherwise, <see langword="false" />.</value>
    internal bool UseEncryption { get; set; } = false;

    /// <summary>
    ///     Gets or sets the AES implementation used by this client.
    /// </summary>
    /// <value>The AES implementation.</value>
    internal Crypt2 Aes { get; set; } = CryptographyUtils.GenerateAes(Array.Empty<byte>());

    /// <inheritdoc />
    public override void Close()
    {
        IsConnected = false;
        State = ClientState.Disconnected;
        base.Close();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        IsConnected = false;
        State = ClientState.Disconnected;
        base.Dispose();
    }

    /// <summary>
    ///     Reads the next packet from the client's stream.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The next packet, or <see langword="null" /> if no valid packet was read.</returns>
    public async Task<Packet?> ReadNextPacketAsync(CancellationToken cancellationToken = default)
    {
        await using var networkStream = new NetworkStream(BaseSocket);
        using var networkReader = new ProtocolReader(networkStream);
        int length;
        try
        {
            length = networkReader.ReadInt32();
        }
        catch (EndOfStreamException)
        {
            State = ClientState.Disconnected;
            throw new DisconnectedException();
        }

        var buffer = new MemoryStream();
        Stream targetStream = buffer;
        buffer.Write(networkReader.ReadBytes(length));
        buffer.Position = 0;

        // if (UseCompression) targetStream = new GZipStream(targetStream, CompressionLevel.Optimal);
        if (UseEncryption)
        {
            var data = new byte[targetStream.Length];
            _ = await targetStream.ReadAsync(data, 0, data.Length, cancellationToken);
            buffer.SetLength(0);
            buffer.Write(Aes.DecryptBytes(data));
            buffer.Position = 0;
        }

        using var bufferReader = new ProtocolReader(targetStream);
        int packetHeader;
        try
        {
            packetHeader = bufferReader.ReadInt32();
        }
        catch (EndOfStreamException)
        {
            State = ClientState.Disconnected;
            throw;
        }

        if (!RegisteredPackets.TryGetValue(packetHeader, out Type? packetType))
        {
            Console.WriteLine($"Unknown packet {packetHeader:X8}");
            return null;
        }

        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        ConstructorInfo? constructor =
            packetType.GetConstructors(bindingFlags).FirstOrDefault(c => c.GetParameters().Length == 0);

        if (constructor is null)
            return null;

        var packet = (Packet)constructor.Invoke(null);
        packet.Deserialize(bufferReader);
        await targetStream.DisposeAsync();

        if (RegisteredPacketHandlers.TryGetValue(packetType, out IReadOnlyCollection<PacketHandler>? handlers))
            await Task.WhenAll(handlers.Select(h => h.HandleAsync(this, packet, cancellationToken)));

        if (_packetCompletionSources.TryGetValue(packet.Id, out List<TaskCompletionSource<Packet>>? completionSources))
        {
            lock (completionSources)
            {
                foreach (TaskCompletionSource<Packet> completionSource in completionSources)
                    completionSource.SetResult(packet);
            }
        }

        return packet;
    }

    /// <summary>
    ///     Sends a packet to the remote endpoint.
    /// </summary>
    /// <param name="packet">The packet to send.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    public async Task SendPacketAsync<TPacket>(TPacket packet, CancellationToken cancellationToken = default)
        where TPacket : Packet
    {
        var buffer = new MemoryStream();
        Stream targetStream = buffer;

        // if (UseCompression) targetStream = new GZipStream(targetStream, CompressionMode.Compress);

        await using var bufferWriter = new ProtocolWriter(targetStream);
        bufferWriter.Write(packet.Id);
        packet.Serialize(bufferWriter);
        await targetStream.FlushAsync(cancellationToken);
        buffer.Position = 0;

        await using var networkStream = new NetworkStream(BaseSocket);
        await using var networkWriter = new ProtocolWriter(networkStream);

        if (UseEncryption)
        {
            byte[] data = buffer.ToArray();
            buffer.SetLength(0);
            buffer.Write(Aes.EncryptBytes(data));
            buffer.Position = 0;
        }

        var length = (int)buffer.Length;
        networkWriter.Write(length);

        try
        {
            await buffer.CopyToAsync(networkStream, cancellationToken);
            await networkStream.FlushAsync(cancellationToken);
        }
        catch (IOException)
        {
            State = ClientState.Disconnected;
            IsConnected = false;

            if (this is ProtocolClient client)
                client.OnDisconnect(DisconnectReason.EndOfStream);
        }
    }

    /// <summary>
    ///     Sends a packet, and waits for a specific packet to be received.
    /// </summary>
    /// <param name="packetToSend">The packet to send.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <typeparam name="TReceive">The type of the packet to return.</typeparam>
    /// <returns>The received packet.</returns>
    /// <remarks>
    ///     This method will consume all incoming packets, raising their associated handlers if such packets are recognised.
    /// </remarks>
    public async Task<TReceive> SendAndReceiveAsync<TReceive>(Packet packetToSend,
        CancellationToken cancellationToken = default)
        where TReceive : Packet
    {
        var attribute = typeof(TReceive).GetCustomAttribute<PacketAttribute>();
        if (attribute is null)
            throw new ArgumentException($"The packet type {typeof(TReceive).Name} is not a valid packet.");

        var requestPacket = packetToSend as RequestPacket;
        if (requestPacket is not null)
            requestPacket.CallbackId = _callbackIdGenerator.GetId(packetToSend, out _);

        var completionSource = new TaskCompletionSource<Packet>();
        if (!_packetCompletionSources.TryGetValue(attribute.Id,
                out List<TaskCompletionSource<Packet>>? completionSources))
        {
            completionSources = new List<TaskCompletionSource<Packet>>();
            _packetCompletionSources.TryAdd(attribute.Id, completionSources);
        }

        lock (completionSources)
        {
            if (!completionSources.Contains(completionSource))
                completionSources.Add(completionSource);
        }

        await SendPacketAsync(packetToSend, cancellationToken);
        TReceive response;
        do
        {
            response = await WaitForPacketAsync<TReceive>(completionSource, cancellationToken);
            if (requestPacket is null)
                break;

            if (response is ResponsePacket responsePacket && responsePacket.CallbackId == requestPacket.CallbackId)
                break;
        } while (true);

        return response;
    }

    /// <summary>
    ///     Waits for a specific packet to be received.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <typeparam name="TPacket">The type of the packet to return.</typeparam>
    /// <returns>The received packet.</returns>
    /// <remarks>
    ///     This method will consume all incoming packets, raising their associated handlers if such packets are recognised.
    /// </remarks>
    public Task<TPacket> WaitForPacketAsync<TPacket>(CancellationToken cancellationToken = default)
        where TPacket : Packet
    {
        var completionSource = new TaskCompletionSource<Packet>();
        return WaitForPacketAsync<TPacket>(completionSource, cancellationToken);
    }

    private async Task<TPacket> WaitForPacketAsync<TPacket>(
        TaskCompletionSource<Packet> completionSource,
        CancellationToken cancellationToken = default
    )
        where TPacket : Packet
    {
        var attribute = typeof(TPacket).GetCustomAttribute<PacketAttribute>();
        if (attribute is null)
            throw new ArgumentException($"The packet type {typeof(TPacket).Name} is not a valid packet.");

        if (!_packetCompletionSources.TryGetValue(attribute.Id,
                out List<TaskCompletionSource<Packet>>? completionSources))
        {
            completionSources = new List<TaskCompletionSource<Packet>>();
            _packetCompletionSources.TryAdd(attribute.Id, completionSources);
        }

        lock (completionSources)
        {
            if (!completionSources.Contains(completionSource))
                completionSources.Add(completionSource);
        }

        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Packet? packet = await ReadNextPacketAsync(cancellationToken);
                    if (packet is TPacket typedPacket)
                    {
                        completionSource.TrySetResult(typedPacket);
                        return;
                    }
                }
                catch (TaskCanceledException)
                {
                    completionSource.SetCanceled();
                }
            }

            completionSource.SetCanceled();
        }, cancellationToken);

        var packet = (TPacket)await Task.Run(() => completionSource.Task, cancellationToken);
        if (_packetCompletionSources.TryGetValue(attribute.Id, out completionSources))
        {
            lock (completionSources)
            {
                completionSources.Remove(completionSource);
                if (completionSources.Count == 0) _packetCompletionSources.TryRemove(attribute.Id, out _);
            }
        }

        return packet;
    }
}
