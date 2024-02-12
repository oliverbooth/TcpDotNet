using System.Buffers.Binary;
using System.Security.Cryptography;

namespace TcpDotNet.Protocol;

/// <summary>
///     Represents a request packet, which forms a request/response packet pair.
/// </summary>
public abstract class RequestPacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestPacket" /> class.
    /// </summary>
    protected RequestPacket()
    {
        Span<byte> buffer = stackalloc byte[8];
        RandomNumberGenerator.Fill(buffer);
        CallbackId = BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    /// <summary>
    ///     Gets the request identifier.
    /// </summary>
    /// <value>The request identifier.</value>
    public long CallbackId { get; private set; }

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
