using TcpDotNet.Protocol.Packets.ServerBound;

namespace TcpDotNet.Protocol.Packets.ClientBound;

/// <summary>
///     Represents a packet which responds to a <see cref="HandshakeRequestPacket" />.
/// </summary>
[Packet(0x7FFFFFE1)]
internal sealed class HandshakeResponsePacket : ResponsePacket
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HandshakeResponsePacket" /> class.
    /// </summary>
    /// <param name="callbackId">The callback ID.</param>
    /// <param name="protocolVersion">The requested protocol version.</param>
    /// <param name="handshakeResponse">The handshake response.</param>
    public HandshakeResponsePacket(long callbackId, int protocolVersion, HandshakeResponse handshakeResponse)
        : base(callbackId)
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
    protected internal override void Deserialize(ProtocolReader reader)
    {
        HandshakeResponse = (HandshakeResponse)reader.ReadByte();
        ProtocolVersion = reader.ReadInt32();
    }

    /// <inheritdoc />
    protected internal override void Serialize(ProtocolWriter writer)
    {
        writer.Write((byte)HandshakeResponse);
        writer.Write(ProtocolVersion);
    }
}
