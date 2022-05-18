namespace TcpDotNet.Protocol;

/// <summary>
///     Represents the base class for a packet handler.
/// </summary>
/// <remarks>This class should be inherited directly, instead </remarks>
public abstract class PacketHandler
{
    /// <summary>
    ///     Handles the specified <see cref="Packet" />.
    /// </summary>
    /// <param name="recipient">The recipient of the packet.</param>
    /// <param name="packet">The packet to handle.</param>
    public abstract Task HandleAsync(BaseClientNode recipient, Packet packet);
}

/// <summary>
///     Represents the base class for a packet handler.
/// </summary>
public abstract class PacketHandler<T> : PacketHandler
    where T : Packet
{
    /// <summary>
    ///     An empty packet handler.
    /// </summary>
    public static readonly PacketHandler<T> Empty = new NullPacketHandler<T>();

    /// <inheritdoc />
    public override Task HandleAsync(BaseClientNode recipient, Packet packet)
    {
        if (packet is T actual) return HandleAsync(recipient, actual);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the specified <see cref="Packet" />.
    /// </summary>
    /// <param name="recipient">The recipient of the packet.</param>
    /// <param name="packet">The packet to handle.</param>
    public abstract Task HandleAsync(BaseClientNode recipient, T packet);
}

/// <summary>
///     Represents a packet handler that does not handle the packet in any meaningful way.
/// </summary>
/// <typeparam name="T">The type of the packet.</typeparam>
internal sealed class NullPacketHandler<T> : PacketHandler<T>
    where T : Packet
{
    /// <inheritdoc />
    public override Task HandleAsync(BaseClientNode recipient, T packet)
    {
        return Task.CompletedTask;
    }
}
