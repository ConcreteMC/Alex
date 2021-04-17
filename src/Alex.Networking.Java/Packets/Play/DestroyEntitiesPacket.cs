using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class DestroyEntitiesPacket : Packet<DestroyEntitiesPacket>
    {
		public DestroyEntitiesPacket()
		{

		}

		public int[] EntityIds;

		public override void Decode(MinecraftStream stream)
		{
			int count = stream.ReadVarInt();
			EntityIds = new int[count];
			for(int i = 0; i < count; i++)
			{
				EntityIds[i] = stream.ReadVarInt();
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			
		}
	}
}
