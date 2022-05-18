using TcpDotNet.Protocol;

namespace TcpDotNet.EventData;

/// <summary>
///     Provides event information for <see cref="ProtocolListener.ClientPacketReceived" />.
/// </summary>
public sealed class ClientPacketReceivedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientPacketReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="client">The client who sent the packet.</param>
    /// <param name="packet">The deserialized packet.</param>
    public ClientPacketReceivedEventArgs(ProtocolListener.Client client, Packet packet)
    {
        Client = client;
        Packet = packet;
    }

    /// <summary>
    ///     Gets the client who sent the packet.
    /// </summary>
    /// <value>The sender.</value>
    public ProtocolListener.Client Client { get; }

    /// <summary>
    ///     Gets the packet which was sent.
    /// </summary>
    /// <value>The packet.</value>
    public Packet Packet { get; }
}
