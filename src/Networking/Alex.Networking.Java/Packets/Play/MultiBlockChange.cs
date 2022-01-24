using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class MultiBlockChange : Packet<MultiBlockChange>
	{
		public int ChunkX;
		public int ChunkZ;

		public BlockUpdate[] Records = null;
		public bool TrustEdges;

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();
			Records = null;
			TrustEdges = false;
		}

		public override void Decode(MinecraftStream stream)
		{
			var chunkSectionPos = stream.ReadLong();
			ChunkX = (int)(chunkSectionPos >> 42);
			var sectionY = (int)(chunkSectionPos << 44 >> 44);
			ChunkZ = (int)(chunkSectionPos << 22 >> 42);

			var inverse = stream.ReadBool();
			TrustEdges = inverse;
			//ChunkX = stream.ReadInt();
			//	ChunkZ = stream.ReadInt();

			int recordCount = stream.ReadVarInt();
			Records = new BlockUpdate[recordCount];

			for (int i = 0; i < Records.Length; i++)
			{
				var encoded = stream.ReadVarLong();

				// long encoded = rawId << 12 | (blockLocalX << 8 | blockLocalZ << 4 | blockLocalY)
				var rawId = encoded >> 12;
				var x = (int)((encoded >> 8) & 0xF);
				var y = (int)(encoded & 0xF);
				var z = (int)((encoded >> 4) & 0xF);

				//byte horizontalPos = (byte)stream.ReadByte();

				BlockUpdate update = new BlockUpdate();
				update.X = (ChunkX << 4) + x;
				update.Z = (ChunkZ << 4) + z;
				update.Y = (sectionY * 16) + y;
				update.BlockId = (uint)rawId;

				Records[i] = update;
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}

		public class BlockUpdate
		{
			public int X;
			public int Y;
			public int Z;

			public uint BlockId;
		}
	}
}