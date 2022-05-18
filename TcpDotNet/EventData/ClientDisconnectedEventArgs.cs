namespace TcpDotNet.EventData;

/// <summary>
///     Provides event information for <see cref="ProtocolListener.ClientDisconnected" />.
/// </summary>
public sealed class ClientDisconnectedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientDisconnectedEventArgs" /> class.
    /// </summary>
    /// <param name="client">The client which disconnected.</param>
    /// <param name="disconnectReason">The reason for the disconnect.</param>
    public ClientDisconnectedEventArgs(ProtocolListener.Client client, DisconnectReason disconnectReason)
    {
        Client = client;
        DisconnectReason = disconnectReason;
    }

    /// <summary>
    ///     Gets the client which connected.
    /// </summary>
    /// <value>The connected client.</value>
    public ProtocolListener.Client Client { get; }

    /// <summary>
    ///     Gets the reason for the disconnect.
    /// </summary>
    /// <value>The reason for the disconnect.</value>
    public DisconnectReason DisconnectReason { get; }
}
