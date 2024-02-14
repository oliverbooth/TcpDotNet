using System.Net;
using TcpDotNet;
using TcpDotNet.ClientIntegrationTest;
using TcpDotNet.ClientIntegrationTest.PacketHandlers;
using TcpDotNet.Protocol;
using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;

using var client = new ProtocolClient();
client.Disconnected += (_, e) =>
{
    Console.WriteLine($"Disconnected: {e.DisconnectReason}");
    return Task.CompletedTask;
};

client.RegisterPacketHandler(PacketHandler<PongPacket>.Empty);
client.RegisterPacketHandler(new GoodbyePacketHandler());

var remoteEP = new IPEndPoint(IPAddress.Loopback, 1234);
Console.WriteLine($"Connecting to {remoteEP}");
await client.ConnectAsync(remoteEP);

Console.WriteLine($"Connected to {client.RemoteEndPoint}. My session is {client.SessionId}");

var ping = new PingPacket();
Console.WriteLine($"Sending ping packet with payload: {BitConverter.ToString(ping.Payload)}");
var pong = await client.SendAndReceiveAsync<PongPacket>(ping);

Console.WriteLine($"Received pong packet with payload: {BitConverter.ToString(pong.Payload)}");
Console.WriteLine(pong.Payload.SequenceEqual(ping.Payload) ? "Payload matches!" : "Payload does not match!");

await client.SendPacketAsync(new HelloPacket {Message = "Hello, world!"});

while (client.IsConnected)
{
    await client.ReadNextPacketAsync();
}
await Task.Delay(-1);
