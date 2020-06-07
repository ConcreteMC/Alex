using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class ServerBoundWindowConfirmationPacket : Packet<ServerBoundWindowConfirmationPacket>
    {
        public ServerBoundWindowConfirmationPacket()
        {
            PacketId = 0x07;
        }
        
        public override void Decode(MinecraftStream stream)
        {
            throw new System.NotImplementedException();
        }

        public override void Encode(MinecraftStream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}