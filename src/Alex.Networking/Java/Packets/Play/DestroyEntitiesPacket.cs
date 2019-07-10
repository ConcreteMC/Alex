using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class DestroyEntitiesPacket : Packet<DestroyEntitiesPacket>
    {
		public DestroyEntitiesPacket()
		{

		}

		public int[] Entitys;

		public override void Decode(MinecraftStream stream)
		{
			int count = stream.ReadVarInt();
			Entitys = new int[count];
			for(int i = 0; i < count; i++)
			{
				Entitys[i] = stream.ReadVarInt();
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			
		}
	}
}
