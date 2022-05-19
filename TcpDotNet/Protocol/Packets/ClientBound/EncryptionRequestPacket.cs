using System.Security.Cryptography;

namespace TcpDotNet.Protocol.Packets.ClientBound;

/// <summary>
///     Represents a packet which requests encryption from the client.
/// </summary>
[Packet(0x7FFFFFE2)]
internal sealed class EncryptionRequestPacket : Packet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EncryptionRequestPacket" /> class.
    /// </summary>
    /// <param name="publicKey">The public key.</param>
    public EncryptionRequestPacket(byte[] publicKey) : this()
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        publicKey ??= Array.Empty<byte>();
        PublicKey = publicKey[..];
    }

    internal EncryptionRequestPacket()
    {
        PublicKey = Array.Empty<byte>();

        using var rng = new RNGCryptoServiceProvider();
        Payload = new byte[64];
        rng.GetBytes(Payload);
    }

    /// <summary>
    ///     Gets the payload.
    /// </summary>
    /// <value>The payload.</value>
    public byte[] Payload { get; private set; }

    /// <summary>
    ///     Gets the public key.
    /// </summary>
    /// <value>The public key.</value>
    public byte[] PublicKey { get; private set; }

    /// <inheritdoc />
    protected internal override Task DeserializeAsync(ProtocolReader reader)
    {
        int length = reader.ReadInt32();
        PublicKey = reader.ReadBytes(length);

        length = reader.ReadInt32();
        Payload = reader.ReadBytes(length);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected internal override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(PublicKey.Length);
        writer.Write(PublicKey);

        writer.Write(Payload.Length);
        writer.Write(Payload);

        return Task.CompletedTask;
    }
}
