using System.Reflection;

namespace TcpDotNet.Protocol;

/// <summary>
///     Represents the base class for all packets on the protocol.
/// </summary>
public abstract class Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Packet" /> class.
    /// </summary>
    protected Packet()
    {
        var attribute = GetType().GetCustomAttribute<PacketAttribute>();
        if (attribute == null)
            throw new InvalidOperationException($"The packet is not decorated with {typeof(PacketAttribute)}.");

        Id = attribute.Id;
    }

    /// <summary>
    ///     Gets the ID of the packet.
    /// </summary>
    /// <value>The packet ID.</value>
    public int Id { get; }

    /// <summary>
    ///     Deserializes this packet from the specified reader.
    /// </summary>
    /// <param name="reader">The reader from which this packet should be deserialized.</param>
    protected internal abstract void Deserialize(ProtocolReader reader);

    /// <summary>
    ///     Serializes this packet to the specified writer.
    /// </summary>
    /// <param name="writer">The writer to which this packet should be serialized.</param>
    protected internal abstract void Serialize(ProtocolWriter writer);
}
