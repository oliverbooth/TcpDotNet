namespace TcpDotNet.Protocol.Packets.ClientBound;

[Packet(0xF1)]
public sealed class PongPacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PongPacket" /> class.
    /// </summary>
    public PongPacket(byte[] payload)
    {
        Payload = payload[..];
    }

    internal PongPacket()
    {
        Payload = Array.Empty<byte>();
    }

    /// <summary>
    ///     Gets the payload in this packet.
    /// </summary>
    /// <value>The payload.</value>
    public byte[] Payload { get; private set; }

    /// <inheritdoc />
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        int length = reader.ReadInt32();
        Payload = reader.ReadBytes(length);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(Payload.Length);
        writer.Write(Payload);
        return Task.CompletedTask;
    }
}
