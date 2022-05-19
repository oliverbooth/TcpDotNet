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
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        Reason = (DisconnectReason) reader.ReadByte();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write((byte) Reason);
        return Task.CompletedTask;
    }
}
