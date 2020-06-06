using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class MultiBlockChange : Packet<MultiBlockChange>
    {
	    public int ChunkX;
	    public int ChunkZ;

	    public BlockUpdate[] Records = null;

		public override void Decode(MinecraftStream stream)
		{
			ChunkX = stream.ReadInt();
			ChunkZ = stream.ReadInt();

			int recordCount = stream.ReadVarInt();
			Records = new BlockUpdate[recordCount];
			for (int i = 0; i < Records.Length; i++)
			{
				byte horizontalPos = (byte)stream.ReadByte();

				BlockUpdate update = new BlockUpdate();
				update.X = (horizontalPos >> 4 & 15) + (ChunkX * 16);
				update.Z = (horizontalPos & 15) + (ChunkZ * 16);
				update.Y = (byte)stream.ReadByte();
				update.BlockId = stream.ReadVarInt();

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

			public int BlockId;
	    }
    }
}
