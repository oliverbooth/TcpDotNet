namespace TcpDotNet.EventData;

/// <summary>
///     Provides event data for the <see cref="ProtocolClient.Disconnected" /> event.
/// </summary>
public sealed class DisconnectedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DisconnectedEventArgs" /> class.
    /// </summary>
    /// <param name="disconnectReason">The reason for the disconnect.</param>
    public DisconnectedEventArgs(DisconnectReason disconnectReason)
    {
        DisconnectReason = disconnectReason;
    }

    /// <summary>
    ///     Gets the reason for the disconnect.
    /// </summary>
    /// <value>The disconnect reason.</value>
    public DisconnectReason DisconnectReason { get; }
}
