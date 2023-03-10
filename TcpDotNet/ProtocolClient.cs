using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using TcpDotNet.EventData;
using TcpDotNet.Protocol;
using TcpDotNet.Protocol.PacketHandlers;
using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;
using Socket = System.Net.Sockets.Socket;
using Task = System.Threading.Tasks.Task;

namespace TcpDotNet;

/// <summary>
///     Represents a client on the TcpDotNet protocol.
/// </summary>
public sealed class ProtocolClient : BaseClientNode
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtocolClient" /> class.
    /// </summary>
    public ProtocolClient()
    {
        RegisterPacketHandler(PacketHandler<HandshakeResponsePacket>.Empty);
        RegisterPacketHandler(PacketHandler<EncryptionRequestPacket>.Empty);
        RegisterPacketHandler(PacketHandler<SessionExchangePacket>.Empty);
        RegisterPacketHandler(new DisconnectPacketHandler());
    }

    /// <summary>
    ///     Occurs when the client has been disconnected.
    /// </summary>
    public event EventHandler<DisconnectedEventArgs>? Disconnected;

    /// <summary>
    ///     Establishes a connection to a remote host.
    /// </summary>
    /// <param name="host">The remote host to which this client should connect.</param>
    /// <param name="port">The remote port to which this client should connect.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="host" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="host" /> contains an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="port" /> is less than <see cref="IPEndPoint.MinPort" />. -or - <paramref name="port" /> is greater
    ///     than <see cref="IPEndPoint.MaxPort" />.
    /// </exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        return ConnectAsync(new DnsEndPoint(host, port), cancellationToken);
    }

    /// <summary>
    ///     Establishes a connection to a remote host.
    /// </summary>
    /// <param name="address">The remote <see cref="IPAddress" /> to which this client should connect.</param>
    /// <param name="port">The remote port to which this client should connect.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="port" /> is less than <see cref="IPEndPoint.MinPort" />. -or - <paramref name="port" /> is greater
    ///     than <see cref="IPEndPoint.MaxPort" />. -or- <paramref name="address" /> is less than 0 or greater than
    ///     0x00000000FFFFFFFF.
    /// </exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public Task ConnectAsync(IPAddress address, int port, CancellationToken cancellationToken = default)
    {
        return ConnectAsync(new IPEndPoint(address, port), cancellationToken);
    }

    /// <summary>
    ///     Establishes a connection to a remote host.
    /// </summary>
    /// <param name="remoteEP">An EndPoint that represents the remote device.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="remoteEP" /> is <see langword="null" />.</exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public async Task ConnectAsync(EndPoint remoteEP, CancellationToken cancellationToken = default)
    {
        if (remoteEP is null) throw new ArgumentNullException(nameof(remoteEP));

        State = ClientState.Connecting;
        BaseSocket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await Task.Run(() => BaseSocket.ConnectAsync(remoteEP), cancellationToken);
        }
        catch
        {
            State = ClientState.None;
            throw;
        }

        IsConnected = true;
        RemoteEndPoint = BaseSocket.RemoteEndPoint;

        State = ClientState.Handshaking;
        var handshakeRequest = new HandshakeRequestPacket(ProtocolVersion);
        var handshakeResponse =
            await SendAndReceiveAsync<HandshakeRequestPacket, HandshakeResponsePacket>(handshakeRequest, cancellationToken);

        if (handshakeResponse.HandshakeResponse != HandshakeResponse.Success)
        {
            Close();
            IsConnected = false;
            throw new InvalidOperationException("Handshake failed. " +
                                                $"Server responded with {handshakeResponse.HandshakeResponse:D}");
        }

        State = ClientState.Encrypting;
        var encryptionRequest = await WaitForPacketAsync<EncryptionRequestPacket>(cancellationToken);
        using var rsa = new RSACryptoServiceProvider(2048);
        rsa.ImportCspBlob(encryptionRequest.PublicKey);
        byte[] encryptedPayload = rsa.Encrypt(encryptionRequest.Payload, true);

        var key = new byte[128];
        using var rng = new RNGCryptoServiceProvider();
        rng.GetBytes(key);

        Aes = CryptographyUtils.GenerateAes(key);
        byte[] aesKey = rsa.Encrypt(key, true);
        var encryptionResponse = new EncryptionResponsePacket(encryptedPayload, aesKey);
        await SendPacketAsync(encryptionResponse, cancellationToken);

        UseEncryption = true;
        var sessionPacket = await WaitForPacketAsync<SessionExchangePacket>(cancellationToken);

        SessionId = sessionPacket.Session;
        State = ClientState.Connected;
    }

    internal void OnDisconnect(DisconnectReason reason)
    {
        Disconnected?.Invoke(this, new DisconnectedEventArgs(reason));
        Close();
    }
}
