using System.Security.Cryptography;

namespace TcpDotNet.Protocol.Packets.ServerBound;

[Packet(0xF0)]
public sealed class PingPacket : Packet
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
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        int length = reader.ReadInt32();
        Payload = reader.ReadBytes(length);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(Payload.Length);
        writer.Write(Payload);
        return Task.CompletedTask;
    }
}
