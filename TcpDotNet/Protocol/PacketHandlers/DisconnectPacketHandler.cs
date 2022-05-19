using TcpDotNet.Protocol.Packets.ClientBound;

namespace TcpDotNet.Protocol.PacketHandlers;

internal sealed class DisconnectPacketHandler : PacketHandler<DisconnectPacket>
{
    /// <inheritdoc />
    public override Task HandleAsync(
        BaseClientNode recipient,
        DisconnectPacket packet,
        CancellationToken cancellationToken = default
    )
    {
        if (recipient is ProtocolClient client)
            client.OnDisconnect(packet.Reason);

        recipient.Close();
        return Task.CompletedTask;
    }
}
