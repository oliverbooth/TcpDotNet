using TcpDotNet;
using TcpDotNet.ListenerIntegrationTest.PacketHandlers;
using TcpDotNet.Protocol;
using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;

var listener = new ProtocolListener();
listener.ClientConnected += (_, e) => Console.WriteLine($"Client connected from {e.Client.RemoteEndPoint} with session {e.Client.SessionId}");
listener.ClientDisconnected += (_, e) => Console.WriteLine($"Client {e.Client.SessionId} disconnected ({e.DisconnectReason})");

listener.RegisterPacketHandler(new HelloPacketHandler());
listener.RegisterPacketHandler(new PingPacketHandler());
listener.Start(1234);

await Task.Delay(-1);

internal sealed class PingPacketHandler : PacketHandler<PingPacket>
{
    public override async Task HandleAsync(BaseClientNode recipient, PingPacket packet, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Client {recipient.SessionId} sent ping with payload {BitConverter.ToString(packet.Payload)}");
        var pong = new PongPacket(packet.CallbackId, packet.Payload);
        await recipient.SendPacketAsync(pong, cancellationToken);
    }
}
