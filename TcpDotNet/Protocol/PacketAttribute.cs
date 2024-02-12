namespace TcpDotNet.Protocol;

/// <summary>
///     Specifies metadata for a <see cref="Packet" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PacketAttribute" /> class.
    /// </summary>
    /// <param name="id">The ID of the packet.</param>
    public PacketAttribute(int id)
    {
        Id = id;
    }

    /// <summary>
    ///     Gets the ID of the packet.
    /// </summary>
    /// <value>The packet ID.</value>
    public int Id { get; }
}
