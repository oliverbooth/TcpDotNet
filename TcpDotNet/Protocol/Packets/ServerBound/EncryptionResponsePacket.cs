using TcpDotNet.Protocol.Packets.ClientBound;

namespace TcpDotNet.Protocol.Packets.ServerBound;

/// <summary>
///     Represents a packet which responds to a <see cref="EncryptionRequestPacket" />.
/// </summary>
[Packet(0xE3)]
internal sealed class EncryptionResponsePacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EncryptionResponsePacket" /> class.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <param name="key">The RSA-encrypted symmetric shared secret.</param>
    public EncryptionResponsePacket(byte[] payload, byte[] key) : this()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Payload = payload?[..] ?? Array.Empty<byte>();
        SharedSecret = key?[..] ?? Array.Empty<byte>();
        // ReSharper enable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }

    internal EncryptionResponsePacket()
    {
        SharedSecret = Array.Empty<byte>();
        Payload = Array.Empty<byte>();
    }

    /// <summary>
    ///     Gets the symmetric shared secret.
    /// </summary>
    /// <value>The RSA-encrypted symmetric shared secret.</value>
    public byte[] SharedSecret { get; private set; }

    /// <summary>
    ///     Gets the payload.
    /// </summary>
    /// <value>The payload.</value>
    public byte[] Payload { get; private set; }


    /// <inheritdoc />
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        int length = reader.ReadInt32();
        SharedSecret = reader.ReadBytes(length);

        length = reader.ReadInt32();
        Payload = reader.ReadBytes(length);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(SharedSecret.Length);
        writer.Write(SharedSecret);

        writer.Write(Payload.Length);
        writer.Write(Payload);

        return Task.CompletedTask;
    }
}
