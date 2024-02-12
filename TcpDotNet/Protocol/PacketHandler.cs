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
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    public abstract Task HandleAsync(ClientNode recipient, Packet packet, CancellationToken cancellationToken = default);
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
    public override Task HandleAsync(ClientNode recipient, Packet packet, CancellationToken cancellationToken = default)
    {
        if (packet is T actual) return HandleAsync(recipient, actual, cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the specified <see cref="Packet" />.
    /// </summary>
    /// <param name="recipient">The recipient of the packet.</param>
    /// <param name="packet">The packet to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    public abstract Task HandleAsync(ClientNode recipient, T packet, CancellationToken cancellationToken = default);
}

/// <summary>
///     Represents a packet handler that does not handle the packet in any meaningful way.
/// </summary>
/// <typeparam name="T">The type of the packet.</typeparam>
internal sealed class NullPacketHandler<T> : PacketHandler<T>
    where T : Packet
{
    /// <inheritdoc />
    public override Task HandleAsync(ClientNode recipient, T packet, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
