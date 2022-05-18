namespace TcpDotNet.Protocol.Packets.ServerBound;

/// <summary>
///     Represents a packet which requests a handshake with a <see cref="ProtocolListener" />.
/// </summary>
[Packet(0x00000001)]
internal sealed class HandshakeRequestPacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HandshakeRequestPacket" /> class.
    /// </summary>
    /// <param name="protocolVersion">The protocol version.</param>
    public HandshakeRequestPacket(int protocolVersion)
    {
        ProtocolVersion = protocolVersion;
    }

    internal HandshakeRequestPacket()
    {
    }

    /// <summary>
    ///     Gets the protocol version in this request.
    /// </summary>
    /// <value>The protocol version.</value>
    public int ProtocolVersion { get; private set; }

    /// <inheritdoc />
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        ProtocolVersion = reader.ReadInt32();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(ProtocolVersion);
        return Task.CompletedTask;
    }
}
