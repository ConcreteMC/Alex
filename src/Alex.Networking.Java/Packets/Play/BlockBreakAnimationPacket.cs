using Alex.Common.Utils.Vectors;
using Alex.Networking.Java.Util;
using Microsoft.Xna.Framework;

namespace Alex.Networking.Java.Packets.Play
{
	public class BlockBreakAnimationPacket : Packet<BlockBreakAnimationPacket>
	{
		public int EntityId { get; set; }
		public BlockCoordinates Position { get; set; }
		public byte DestroyStage { get; set; }
		
		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Position = stream.ReadBlockCoordinates();
			DestroyStage = (byte) stream.ReadByte();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}