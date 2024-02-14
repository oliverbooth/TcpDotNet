using TcpDotNet.Protocol;

namespace TcpDotNet.ListenerIntegrationTest;

[Packet(0x01)]
internal sealed class HelloPacket : Packet
{
    public string Message { get; set; }

    protected override void Deserialize(ProtocolReader reader)
    {
        Message = reader.ReadString();
    }

    protected override void Serialize(ProtocolWriter writer)
    {
        writer.Write(Message);
    }
}
