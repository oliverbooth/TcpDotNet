using TcpDotNet.Protocol.Packets.ServerBound;

namespace TcpDotNet.Protocol.Packets.ClientBound;

/// <summary>
///     Represents a packet which responds to a <see cref="HandshakeRequestPacket" />.
/// </summary>
[Packet(0xE1)]
internal sealed class HandshakeResponsePacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HandshakeResponsePacket" /> class.
    /// </summary>
    /// <param name="protocolVersion">The requested protocol version.</param>
    /// <param name="handshakeResponse">The handshake response.</param>
    public HandshakeResponsePacket(int protocolVersion, HandshakeResponse handshakeResponse)
    {
        ProtocolVersion = protocolVersion;
        HandshakeResponse = handshakeResponse;
    }

    internal HandshakeResponsePacket()
    {
    }

    /// <summary>
    ///     Gets the handshake response.
    /// </summary>
    /// <value>The handshake response.</value>
    public HandshakeResponse HandshakeResponse { get; private set; }

    /// <summary>
    ///     Gets the requested protocol version.
    /// </summary>
    /// <value>The protocol version.</value>
    public int ProtocolVersion { get; private set; }

    /// <inheritdoc />
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        HandshakeResponse = (HandshakeResponse) reader.ReadByte();
        ProtocolVersion = reader.ReadInt32();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write((byte) HandshakeResponse);
        writer.Write(ProtocolVersion);
        return Task.CompletedTask;
    }
}
