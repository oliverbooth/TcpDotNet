namespace TcpDotNet;

/// <summary>
///     An enumeration of disconnect reasons.
/// </summary>
public enum DisconnectReason
{
    /// <summary>
    ///     The client disconnected gracefully.
    /// </summary>
    Disconnect,

    /// <summary>
    ///     The client reached an unexpected end of stream.
    /// </summary>
    EndOfStream,

    /// <summary>
    ///     The client sent an invalid encryption payload.
    /// </summary>
    InvalidEncryptionKey
}
