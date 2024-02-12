using TcpDotNet.Protocol;

namespace TcpDotNet.ListenerIntegrationTest;

[Packet(0x02)]
internal sealed class GoodbyePacket : Packet
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
