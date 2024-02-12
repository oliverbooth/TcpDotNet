namespace TcpDotNet.Protocol;

/// <summary>
///     An enumeration of handshake responses.
/// </summary>
public enum HandshakeResponse : byte
{
    Success = 0x00,
    UnsupportedProtocolVersion = 0x01
}
