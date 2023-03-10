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
    protected internal override void Deserialize(ProtocolReader reader)
    {
        CallbackId = reader.ReadInt64();
    }

    /// <inheritdoc />
    protected internal override void Serialize(ProtocolWriter writer)
    {
        writer.Write(CallbackId);
    }
}
