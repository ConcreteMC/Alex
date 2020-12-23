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
			var chunkSectionPos = stream.ReadLong();
			ChunkX = (int) (chunkSectionPos >> 42);
			var sectionY = (int)(chunkSectionPos << 44 >> 44);
			ChunkZ = (int) (chunkSectionPos << 22 >> 42);

			var inverse = stream.ReadBool();
			//ChunkX = stream.ReadInt();
		//	ChunkZ = stream.ReadInt();

			int recordCount = stream.ReadVarInt();
			Records = new BlockUpdate[recordCount];
			for (int i = 0; i < Records.Length; i++)
			{
				var l     = stream.ReadVarLong();
				
				var rawId = l >> 12;

				//var coordinates = l << 52 >> 52;
				//var x           = coordinates >> 8;
				//var z = coordinates << 
				var x     = (int)(l << 4 >> 60);
				var z     = (int)(l << 8 >> 4);
				var y     = (int)(l << 12 >> 12);
				
				//byte horizontalPos = (byte)stream.ReadByte();

				BlockUpdate update = new BlockUpdate();
				update.X = (x & 15) + (ChunkX * 16);
				update.Z = (z & 15) + (ChunkZ * 16);
				update.Y = (y & 15) + (sectionY * 16);
				update.BlockId = (uint) rawId;

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
