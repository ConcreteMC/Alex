using Alex.Common.Utils.Vectors;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class BlockActionPacket : Packet<BlockActionPacket>
	{
		public BlockCoordinates Location { get; set; }
		public byte ActionId { get; set; }
		public byte Parameter { get; set; }
		public int BlockType { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			Location = stream.ReadBlockCoordinates();
			ActionId = (byte)stream.ReadByte();
			Parameter = (byte)stream.ReadByte();
			BlockType = stream.ReadVarInt();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}