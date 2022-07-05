using TcpDotNet.Protocol;

namespace TcpDotNet.ClientIntegrationTest.PacketHandlers;

internal sealed class GoodbyePacketHandler : PacketHandler<GoodbyePacket>
{
    public override Task HandleAsync(BaseClientNode recipient, GoodbyePacket packet, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Server sent {packet.Message}");
        return Task.CompletedTask;
    }
}
