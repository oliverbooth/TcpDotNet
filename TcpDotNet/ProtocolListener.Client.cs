using System.Net.Sockets;
using TcpDotNet.Protocol;

namespace TcpDotNet;

public sealed partial class ProtocolListener
{
    /// <summary>
    ///     Represents a client that is connected to a <see cref="ProtocolListener" />.
    /// </summary>
    public sealed class Client : BaseClientNode
    {
        internal Client(ProtocolListener listener, Socket socket)
        {
            ParentListener = listener ?? throw new ArgumentNullException(nameof(listener));
            BaseSocket = socket ?? throw new ArgumentNullException(nameof(socket));
            SessionId = Guid.NewGuid();
            IsConnected = true;
            RemoteEndPoint = socket.RemoteEndPoint;
        }

        /// <summary>
        ///     Gets the listener to which this client is connected.
        /// </summary>
        /// <value>The parent listener.</value>
        public ProtocolListener ParentListener { get; }

        /// <summary>
        ///     Gets or sets the client's verification payload.
        /// </summary>
        internal byte[] AesVerificationPayload { get; set; } = Array.Empty<byte>();

        internal void Start()
        {
            foreach (Type packetType in ParentListener.RegisteredPackets.Values)
                RegisterPacket(packetType);

            foreach ((Type packetType, IReadOnlyCollection<PacketHandler>? handlers) in ParentListener.RegisteredPacketHandlers)
            foreach (PacketHandler handler in handlers)
                RegisterPacketHandler(packetType, handler);

            State = ClientState.Handshaking;
            Task.Run(ReadLoopAsync);
        }

        private async Task ReadLoopAsync()
        {
            while (IsConnected)
            {
                try
                {
                    Packet? packet = await ReadNextPacketAsync();
                    if (packet is not null) ParentListener.OnClientSendPacket(this, packet);
                }
                catch (DisconnectedException)
                {
                    ParentListener.OnClientDisconnect(this, DisconnectReason.Disconnect);
                    IsConnected = false;
                }
                catch (EndOfStreamException)
                {
                    ParentListener.OnClientDisconnect(this, DisconnectReason.EndOfStream);
                    IsConnected = false;
                }
            }
        }
    }
}
