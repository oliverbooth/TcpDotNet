namespace TcpDotNet.Protocol;

/// <summary>
///     Represents a request packet, which forms a request/response packet pair.
/// </summary>
public abstract class RequestPacket : Packet
{
    /// <summary>
    ///     Gets the request identifier.
    /// </summary>
    /// <value>The request identifier.</value>
    public long CallbackId { get; internal set; }

    /// <inheritdoc />
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        CallbackId = reader.ReadInt64();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(CallbackId);
        return Task.CompletedTask;
    }
}
