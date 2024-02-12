using System.Security.Cryptography;

namespace TcpDotNet.Protocol.Packets.ServerBound;

/// <summary>
///     Represents a packet which performs a heartbeat request.
/// </summary>
[Packet(0x7FFFFFF0)]
public sealed class PingPacket : RequestPacket
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PingPacket" /> class.
    /// </summary>
    public PingPacket()
    {
        using var rng = new RNGCryptoServiceProvider();
        Payload = new byte[4];
        rng.GetBytes(Payload);
    }

    /// <summary>
    ///     Gets the payload in this packet.
    /// </summary>
    /// <value>The payload.</value>
    public byte[] Payload { get; private set; }

    /// <inheritdoc />
    protected internal override void Deserialize(ProtocolReader reader)
    {
        base.Deserialize(reader);
        int length = reader.ReadInt32();
        Payload = reader.ReadBytes(length);
    }

    /// <inheritdoc />
    protected internal override void Serialize(ProtocolWriter writer)
    {
        base.Serialize(writer);
        writer.Write(Payload.Length);
        writer.Write(Payload);
    }
}
