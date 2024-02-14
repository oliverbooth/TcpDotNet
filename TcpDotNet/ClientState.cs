namespace TcpDotNet;

/// <summary>
///     An enumeration of states for a <see cref="ClientNode" /> to be in.
/// </summary>
public enum ClientState
{
    /// <summary>
    ///     The client is not connected to a remote server.
    /// </summary>
    None,

    /// <summary>
    ///     The client has disconnected from a previously-connected server.
    /// </summary>
    Disconnected,

    /// <summary>
    ///     The client is establishing a connection to a remote server.
    /// </summary>
    Connecting,

    /// <summary>
    ///     The client is handshaking.
    /// </summary>
    Handshaking,

    /// <summary>
    ///     The client is exchanging encryption keys.
    /// </summary>
    Encrypting,

    /// <summary>
    ///     The client is connected.
    /// </summary>
    Connected
}
