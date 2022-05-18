namespace TcpDotNet.EventData;

/// <summary>
///     Provides event information for <see cref="ProtocolListener.ClientConnected" />.
/// </summary>
public sealed class ClientConnectedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClientConnectedEventArgs" /> class.
    /// </summary>
    /// <param name="client">The client which connected.</param>
    public ClientConnectedEventArgs(ProtocolListener.Client client)
    {
        Client = client;
    }

    /// <summary>
    ///     Gets the client which connected.
    /// </summary>
    /// <value>The connected client.</value>
    public ProtocolListener.Client Client { get; }
}
