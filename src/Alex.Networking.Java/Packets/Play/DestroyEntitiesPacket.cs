using System.Threading.Tasks;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class DestroyEntitiesPacket : Packet<DestroyEntitiesPacket>
	{
		public DestroyEntitiesPacket() { }

		public int[] EntityIds;

		/// <inheritdoc />
		public override async Task DecodeAsync(MinecraftStream stream)
		{
			int count = await stream.ReadVarIntAsync();
			EntityIds = new int[count];

			for (int i = 0; i < EntityIds.Length; i++)
			{
				EntityIds[i] = await stream.ReadVarIntAsync();
			}
		}

		public override void Encode(MinecraftStream stream) { }
	}
}