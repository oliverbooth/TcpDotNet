using TcpDotNet.Protocol;

namespace TcpDotNet.ListenerIntegrationTest.PacketHandlers;

internal sealed class HelloPacketHandler : PacketHandler<HelloPacket>
{
    public override Task HandleAsync(ClientNode recipient, HelloPacket packet, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Client sent {packet.Message}");
        return recipient.SendPacketAsync(new GoodbyePacket {Message = "Goodbye!"}, cancellationToken);
    }
}
