using TcpDotNet.Protocol;

namespace TcpDotNet.ClientIntegrationTest;

[Packet(0x01)]
internal sealed class HelloPacket : Packet
{
    public string Message { get; set; }

    protected override Task DeserializeAsync(ProtocolReader reader)
    {
        Message = reader.ReadString();
        return Task.CompletedTask;
    }

    protected override Task SerializeAsync(ProtocolWriter writer)
    {
        writer.Write(Message);
        return Task.CompletedTask;
    }
}
