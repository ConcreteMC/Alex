using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class UpdateViewPositionPacket : Packet<UpdateViewPositionPacket>
	{
		public int ChunkX { get; set; }
		public int ChunkZ { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			ChunkX = stream.ReadVarInt();
			ChunkZ = stream.ReadVarInt();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}