using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;

namespace TcpDotNet.Protocol.PacketHandlers;

/// <summary>
///     Represents a handler for a <see cref="HandshakeRequestPacket" />.
/// </summary>
internal sealed class HandshakeRequestPacketHandler : PacketHandler<HandshakeRequestPacket>
{
    /// <inheritdoc />
    public override async Task HandleAsync(
        ClientNode recipient,
        HandshakeRequestPacket packet,
        CancellationToken cancellationToken = default
    )
    {
        if (recipient is not ProtocolListener.Client client)
            return;

        HandshakeResponsePacket response;

        if (packet.ProtocolVersion != Node.ProtocolVersion)
        {
            const HandshakeResponse responseCode = HandshakeResponse.UnsupportedProtocolVersion;
            response = new HandshakeResponsePacket(packet.CallbackId, packet.ProtocolVersion, responseCode);
            await client.SendPacketAsync(response, cancellationToken);
            client.Close();
            return;
        }

        response = new HandshakeResponsePacket(packet.CallbackId, packet.ProtocolVersion, HandshakeResponse.Success);
        await client.SendPacketAsync(response, cancellationToken);

        client.State = ClientState.Encrypting;
        await Task.Delay(1000, cancellationToken);

        var encryptionRequest = new EncryptionRequestPacket(client.ParentListener.Rsa.ExportCspBlob(false));
        client.AesVerificationPayload = encryptionRequest.Payload;
        await client.SendPacketAsync(encryptionRequest, cancellationToken);
    }
}
