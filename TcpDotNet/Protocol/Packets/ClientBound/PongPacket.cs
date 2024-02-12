namespace TcpDotNet.Protocol.Packets.ClientBound;

/// <summary>
///     Represents a packet which performs a heartbeat response.
/// </summary>
[Packet(0x7FFFFFF1)]
public sealed class PongPacket : ResponsePacket
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PongPacket" /> class.
    /// </summary>
    public PongPacket(long callbackId, byte[] payload)
        : base(callbackId)
    {
        Payload = payload[..];
    }

    internal PongPacket() : base(0)
    {
        Payload = Array.Empty<byte>();
    }

    /// <summary>
    ///     Gets the payload in this packet.
    /// </summary>
    /// <value>The payload.</value>
    public byte[] Payload { get; private set; }

    /// <inheritdoc />
    protected internal override void Deserialize(ProtocolReader reader)
    {
        base.Deserialize(reader);
        int length = reader.ReadInt32();
        Payload = reader.ReadBytes(length);
    }

    /// <inheritdoc />
    protected internal override void Serialize(ProtocolWriter writer)
    {
        base.Serialize(writer);
        writer.Write(Payload.Length);
        writer.Write(Payload);
    }
}
