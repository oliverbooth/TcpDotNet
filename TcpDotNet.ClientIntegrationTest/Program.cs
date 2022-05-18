using System.Net;
using TcpDotNet;
using TcpDotNet.Protocol;
using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;

using var client = new ProtocolClient();
client.RegisterPacketHandler(PacketHandler<PongPacket>.Empty);
await client.ConnectAsync(IPAddress.IPv6Loopback, 1234);

Console.WriteLine($"Connected to {client.RemoteEndPoint}");
var ping = new PingPacket();

Console.WriteLine($"Sending ping packet with payload: {BitConverter.ToString(ping.Payload)}");
var pong = await client.SendAndReceive<PingPacket, PongPacket>(ping);

Console.WriteLine($"Received pong packet with payload: {BitConverter.ToString(pong.Payload)}");
Console.WriteLine(pong.Payload.SequenceEqual(ping.Payload) ? "Payload matches!" : "Payload does not match!");
