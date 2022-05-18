using System.Net;
using TcpDotNet;
using TcpDotNet.Protocol;
using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;

using var client = new ProtocolClient();
client.RegisterPacketHandler(PacketHandler<PongPacket>.Empty);
await client.ConnectAsync(IPAddress.IPv6Loopback, 1234);

Console.WriteLine($"Connected to {client.RemoteEndPoint}");
Console.WriteLine("Sending ping packet...");
var ping = new PingPacket();
await client.SendPacketAsync(ping);

Console.WriteLine("Waiting for response...");
Packet? response = await client.ReadNextPacketAsync();
if (response is PongPacket pong)
{
    Console.WriteLine("Received pong packet");
    Console.WriteLine(pong.Payload.SequenceEqual(ping.Payload) ? "Payload matches" : "Payload does not match");
}
else
{
    Console.WriteLine("Received unknown packet");
}
