namespace TcpDotNet.Protocol;

/// <summary>
///     Represents a response packet, which forms a request/response packet pair.
/// </summary>
public abstract class ResponsePacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ResponsePacket" /> class.
    /// </summary>
    /// <param name="callbackId">The callback ID.</param>
    protected ResponsePacket(long callbackId)
    {
        CallbackId = callbackId;
    }

    /// <summary>
    ///     Gets the response identifier.
    /// </summary>
    /// <value>The response identifier.</value>
    public long CallbackId { get; private set; }

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
