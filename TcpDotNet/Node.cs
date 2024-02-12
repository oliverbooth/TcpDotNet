using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Reflection;
using TcpDotNet.Protocol;

namespace TcpDotNet;

/// <summary>
///     Represents a TCP node.
/// </summary>
public abstract class Node : IDisposable
{
    /// <summary>
    ///     The protocol version of this node.
    /// </summary>
    public const int ProtocolVersion = 1;

    private readonly ConcurrentDictionary<int, Type> _registeredPackets = new();
    private readonly ConcurrentDictionary<Type, List<PacketHandler>> _registeredPacketHandlers = new();

    /// <summary>
    ///     Gets the underlying socket for this node.
    /// </summary>
    /// <value>The underlying socket.</value>
    public Socket BaseSocket { get; protected set; } = new(SocketType.Stream, ProtocolType.Tcp);

    /// <summary>
    ///     Gets the registered packets for this node.
    /// </summary>
    /// <value>The registered packets.</value>
    public IReadOnlyDictionary<int, Type> RegisteredPackets =>
        new ReadOnlyDictionary<int, Type>(_registeredPackets);

    /// <summary>
    ///     Gets the registered packets for this node.
    /// </summary>
    /// <value>The registered packets.</value>
    public IReadOnlyDictionary<Type, IReadOnlyCollection<PacketHandler>> RegisteredPacketHandlers =>
        new ReadOnlyDictionary<Type, IReadOnlyCollection<PacketHandler>>(
            _registeredPacketHandlers.ToDictionary(p => p.Key,
                p => (IReadOnlyCollection<PacketHandler>)p.Value.AsReadOnly()));

    /// <summary>
    ///     Closes the base socket connection and releases all associated resources.
    /// </summary>
    public virtual void Close()
    {
        BaseSocket.Close();
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        BaseSocket.Dispose();
    }

    /// <summary>
    ///     Registers a packet handler.
    /// </summary>
    /// <param name="packetType">The type of the packet to handle.</param>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="handler" /> or <paramref name="packetType" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">The type of <paramref name="packetType" /> is not a valid packet.</exception>
    public void RegisterPacketHandler(Type packetType, PacketHandler handler)
    {
        if (packetType is null) throw new ArgumentNullException(nameof(packetType));
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        RegisterPacket(packetType);

        if (!_registeredPacketHandlers.TryGetValue(packetType, out List<PacketHandler>? handlers))
        {
            handlers = new List<PacketHandler>();
            _registeredPacketHandlers.TryAdd(packetType, handlers);
        }

        handlers.Add(handler);
    }

    /// <summary>
    ///     Registers a packet handler.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="handler" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The type of <typeparamref name="TPacket" /> is not a valid packet.</exception>
    public void RegisterPacketHandler<TPacket>(PacketHandler<TPacket> handler)
        where TPacket : Packet
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        RegisterPacket<TPacket>();

        if (!_registeredPacketHandlers.TryGetValue(typeof(TPacket), out List<PacketHandler>? handlers))
        {
            handlers = new List<PacketHandler>();
            _registeredPacketHandlers.TryAdd(typeof(TPacket), handlers);
        }

        handlers.Add(handler);
    }

    /// <summary>
    ///     Registers a packet.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <exception cref="ArgumentException">The type of <typeparamref name="TPacket" /> is not a valid packet.</exception>
    internal void RegisterPacket<TPacket>()
    {
        RegisterPacket(typeof(TPacket));
    }

    /// <summary>
    ///     Registers a packet.
    /// </summary>
    /// <param name="packetType">The type of the packet.</param>
    /// <exception cref="ArgumentException">The type of <paramref name="packetType" /> is not a valid packet.</exception>
    internal void RegisterPacket(Type packetType)
    {
        if (_registeredPackets.Values.Contains(packetType)) return;
        if (!packetType.IsSubclassOf(typeof(Packet)))
            throw new ArgumentException("The type of the packet is not a valid packet.", nameof(packetType));

        var attribute = packetType.GetCustomAttribute<PacketAttribute>();
        if (attribute is null) throw new ArgumentException($"{packetType.Name} is not a valid packet.");
        if (_registeredPackets.TryGetValue(attribute.Id, out Type? registeredPacket))
            throw new ArgumentException(
                $"The packet type {attribute.Id:X8} is already registered to {registeredPacket.Name}.");

        _registeredPackets.TryAdd(attribute.Id, packetType);
    }
}
