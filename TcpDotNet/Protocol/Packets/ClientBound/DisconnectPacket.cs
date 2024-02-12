namespace TcpDotNet.Protocol.Packets.ClientBound;

[Packet(0x7FFFFFFF)]
internal sealed class DisconnectPacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DisconnectPacket" /> class.
    /// </summary>
    /// <param name="reason">The reason for the disconnect.</param>
    public DisconnectPacket(DisconnectReason reason)
    {
        Reason = reason;
    }

    internal DisconnectPacket()
    {
    }

    /// <summary>
    ///     Gets the reason for the disconnect.
    /// </summary>
    public DisconnectReason Reason { get; private set; }

    /// <inheritdoc />
    protected internal override void Deserialize(ProtocolReader reader)
    {
        Reason = (DisconnectReason)reader.ReadByte();
    }

    /// <inheritdoc />
    protected internal override void Serialize(ProtocolWriter writer)
    {
        writer.Write((byte)Reason);
    }
}
