using System.Net;
using TcpDotNet;
using TcpDotNet.Protocol;
using TcpDotNet.Protocol.Packets.ClientBound;
using TcpDotNet.Protocol.Packets.ServerBound;

using var client = new ProtocolClient();
client.Disconnected += (_, e) => Console.WriteLine($"Disconnected: {e.DisconnectReason}");

new Thread(() =>
{
    ClientState oldState = client.State;
    while (true)
    {
        if (oldState != client.State)
        {
            Console.WriteLine($"State changed to {client.State}");
            oldState = client.State;
        }
    }
}).Start();

client.RegisterPacketHandler(PacketHandler<PongPacket>.Empty);
await client.ConnectAsync(IPAddress.IPv6Loopback, 1234);

Console.WriteLine($"Connected to {client.RemoteEndPoint}. My session is {client.SessionId}");
var cancellationTokenSource = new CancellationTokenSource();
cancellationTokenSource.CancelAfter(5000); // if no pong is received in 5 seconds, cancel

var ping = new PingPacket();
Console.WriteLine($"Sending ping packet with payload: {BitConverter.ToString(ping.Payload)}");
var pong = await client.SendAndReceive<PingPacket, PongPacket>(ping, cancellationTokenSource.Token);

Console.WriteLine($"Received pong packet with payload: {BitConverter.ToString(pong.Payload)}");
Console.WriteLine(pong.Payload.SequenceEqual(ping.Payload) ? "Payload matches!" : "Payload does not match!");
