using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using TcpDotNet.EventData;
using TcpDotNet.Protocol;
using TcpDotNet.Protocol.PacketHandlers;
using TcpDotNet.Protocol.Packets.ClientBound;

namespace TcpDotNet;

/// <summary>
///     Represents a listener on the TcpDotNet protocol.
/// </summary>
public sealed partial class ProtocolListener : Node
{
    private readonly List<Client> _clients = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtocolListener" /> class.
    /// </summary>
    public ProtocolListener()
    {
        RegisterPacketHandler(new HandshakeRequestPacketHandler());
        RegisterPacketHandler(new EncryptionResponsePacketHandler());
    }

    /// <summary>
    ///     Occurs when a client connects to the listener.
    /// </summary>
    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    /// <summary>
    ///     Occurs when a client disconnects from the listener.
    /// </summary>
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

    /// <summary>
    ///     Occurs when a client sends a packet to the listener.
    /// </summary>
    public event EventHandler<ClientPacketReceivedEventArgs>? ClientPacketReceived;

    /// <summary>
    ///     Occurs when the server has started.
    /// </summary>
    public event EventHandler? Started;

    /// <summary>
    ///     Occurs when the server has started.
    /// </summary>
    public event EventHandler? Stopped;

    /// <summary>
    ///     Gets a read-only view of the clients connected to this listener.
    /// </summary>
    /// <value>A read-only view of the clients connected to this listener.</value>
    public IReadOnlyCollection<Client> Clients
    {
        get
        {
            lock (_clients)
                return _clients.AsReadOnly();
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the server is running.
    /// </summary>
    /// <value><see langword="true" /> if the server is running; otherwise, <see langword="false" />.</value>
    public bool IsRunning { get; private set; }

    /// <summary>
    ///     Gets the local endpoint.
    /// </summary>
    /// <value>The <see cref="EndPoint" /> that <see cref="Node.BaseSocket" /> is using for communications.</value>
    public EndPoint LocalEndPoint => BaseSocket.LocalEndPoint;

    /// <summary>
    ///     Gets the RSA provider for this listener.
    /// </summary>
    /// <value>The RSA provider.</value>
    internal RSACryptoServiceProvider Rsa { get; } = new(2048);

    /// <summary>
    ///     Starts the listener on the specified port, using <see cref="IPAddress.Any" /> as the bind address, or
    ///     <see cref="IPAddress.IPv6Any" /> if <see cref="Socket.OSSupportsIPv6" /> is <see langword="true" />. 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <paramref name="port" /> is less than <see cref="IPEndPoint.MinPort" /> or greater than
    ///     <see cref="IPEndPoint.MaxPort" />.
    /// </exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public void Start(int port)
    {
        IPAddress bindAddress = Socket.OSSupportsIPv6 ? IPAddress.IPv6Any : IPAddress.Any;
        Start(new IPEndPoint(bindAddress, port));
    }

    /// <summary>
    ///     Starts the listener on the specified endpoint.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="localEP" /> is <see langword="null" />.</exception>
    /// <exception cref="SocketException">An error occurred when attempting to access the socket.</exception>
    public void Start(EndPoint localEP)
    {
        if (localEP is null) throw new ArgumentNullException(nameof(localEP));
        BaseSocket = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        if (localEP.AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6)
            BaseSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

        BaseSocket.Bind(localEP);
        BaseSocket.Listen(10);
        IsRunning = true;

        Started?.Invoke(this, EventArgs.Empty);
        Task.Run(AcceptLoop);
    }

    internal void OnClientConnect(Client client)
    {
        lock (_clients) _clients.Add(client);
        ClientConnected?.Invoke(this, new ClientConnectedEventArgs(client));
    }

    internal void OnClientDisconnect(Client client, DisconnectReason disconnectReason)
    {
        var disconnectPacket = new DisconnectPacket(disconnectReason);
        client.SendPacketAsync(disconnectPacket).GetAwaiter().GetResult();
        client.Close();
        lock (_clients) _clients.Remove(client);
        ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(client, disconnectReason));
    }

    private void OnClientSendPacket(Client client, Packet packet)
    {
        ClientPacketReceived?.Invoke(this, new ClientPacketReceivedEventArgs(client, packet));
    }

    private async Task AcceptLoop()
    {
        while (IsRunning)
        {
            Socket socket = await BaseSocket.AcceptAsync();

            var client = new Client(this, socket);
            client.Start();
        }
    }
}
