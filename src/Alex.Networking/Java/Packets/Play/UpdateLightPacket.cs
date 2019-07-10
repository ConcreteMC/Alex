using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class UpdateLightPacket : Packet<UpdateLightPacket>
    {
	    public int ChunkX,
		    ChunkZ,
		    SkyLightMask,
		    BlockLightMask,
		    EmptySkyLightMask,
		    EmptyBlockLightMask;

	    public byte[][] SkyLightArrays;
	    public byte[][] BlockLightArrays;

        public UpdateLightPacket()
	    {

	    }

	    public override void Decode(MinecraftStream stream)
	    {
		    ChunkX = stream.ReadVarInt();
		    ChunkZ = stream.ReadVarInt();
		    SkyLightMask = stream.ReadVarInt();
		    BlockLightMask = stream.ReadVarInt();
		    EmptySkyLightMask = stream.ReadVarInt();
		    EmptyBlockLightMask = stream.ReadVarInt();

		  //  List<byte[]> skyLightList = new List<byte[]>();
			SkyLightArrays = new byte[18][];
		    for(int i = 0; i < 18; i++)
		    {
			    if (((SkyLightMask & 1) == 1) /*&& ((EmptySkyLightMask & 1) == 0)*/)
			    {
					byte[] data = stream.Read(2048);
				    SkyLightArrays[i] = data;
			    }
			    else
			    {
				    SkyLightArrays[i] = null;
			    }

			    SkyLightMask = SkyLightMask >> 1;
			    EmptySkyLightMask = EmptySkyLightMask >> 1;
            }
            //SkyLightArrays = skyLightList.ToArray();

            // List<byte[]> blockLightList = new List<byte[]>();
		    BlockLightArrays = new byte[18][];
            for (int i = 0; i < 18; i++)
		    {
			    if (((BlockLightMask & 1) == 1) /*&& ((EmptyBlockLightMask & 1) == 0)*/)
			    {
				    byte[] data = stream.Read(2048);
				    BlockLightArrays[i] = data;
			    }
			    else
			    {
				    BlockLightArrays[i] = null;
			    }

                BlockLightMask = BlockLightMask >> 1;
			    EmptyBlockLightMask = EmptyBlockLightMask >> 1;
		    }
		   // BlockLightArrays = blockLightList.ToArray();
        }

	    public override void Encode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }
    }
}
