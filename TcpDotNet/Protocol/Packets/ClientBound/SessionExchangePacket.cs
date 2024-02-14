namespace TcpDotNet.Protocol.Packets.ClientBound;

[Packet(0x7FFFFFE4)]
internal sealed class SessionExchangePacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionExchangePacket" /> class.
    /// </summary>
    /// <param name="session">The session.</param>
    public SessionExchangePacket(Guid session)
    {
        Session = session;
    }

    internal SessionExchangePacket()
    {
        Session = Guid.Empty;
    }

    /// <summary>
    ///     Gets the session.
    /// </summary>
    /// <value>The session.</value>
    public Guid Session { get; private set; }

    /// <inheritdoc />
    protected internal override void Deserialize(ProtocolReader reader)
    {
        Session = reader.ReadGuid();
    }

    /// <inheritdoc />
    protected internal override void Serialize(ProtocolWriter writer)
    {
        writer.Write(Session);
    }
}
