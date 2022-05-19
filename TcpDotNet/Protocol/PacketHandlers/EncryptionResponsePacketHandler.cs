using System.Security.Cryptography;
using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;

namespace TcpDotNet.Protocol.PacketHandlers;

/// <summary>
///     Represents a handler for a <see cref="EncryptionResponsePacket" />.
/// </summary>
internal sealed class EncryptionResponsePacketHandler : PacketHandler<EncryptionResponsePacket>
{
    /// <inheritdoc />
    public override async Task HandleAsync(
        BaseClientNode recipient,
        EncryptionResponsePacket packet,
        CancellationToken cancellationToken = default
    )
    {
        if (recipient is not ProtocolListener.Client client)
            return;

        RSACryptoServiceProvider rsa = client.ParentListener.Rsa;
        byte[] payload = rsa.Decrypt(packet.Payload, true);
        if (!payload.SequenceEqual(client.AesVerificationPayload))
        {
            client.ParentListener.OnClientDisconnect(client, DisconnectReason.InvalidEncryptionKey);
            return;
        }

        byte[] key = rsa.Decrypt(packet.SharedSecret, true);
        client.AesVerificationPayload = Array.Empty<byte>();
        client.Aes = CryptographyUtils.GenerateAes(key);
        client.State = ClientState.Connected;
        client.ParentListener.OnClientConnect(client);

        var sessionPacket = new SessionExchangePacket(client.SessionId);
        await client.SendPacketAsync(sessionPacket, cancellationToken);

        client.UseEncryption = true;
    }
}
