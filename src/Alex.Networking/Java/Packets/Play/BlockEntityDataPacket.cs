using Alex.API.Utils;
using Alex.Networking.Java.Util;
using fNbt;

namespace Alex.Networking.Java.Packets.Play
{
	public class BlockEntityDataPacket : Packet<BlockEntityDataPacket>
	{
		public BlockCoordinates Location { get; set; }
		public byte Action { get; set; }
		public NbtCompound Compound { get; set; }
		
		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			Location = stream.ReadPosition();
			Action = (byte) stream.ReadByte();
			Compound = stream.ReadNbtCompound();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}