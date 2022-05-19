namespace TcpDotNet.Protocol.Packets.ClientBound;

[Packet(0xE4)]
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
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        Session = reader.ReadGuid();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(Session);
        return Task.CompletedTask;
    }
}
