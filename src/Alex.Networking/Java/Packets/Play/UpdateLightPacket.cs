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

	    public bool TrustEdges;

	    public byte[][] SkyLightArrays;
	    public byte[][] BlockLightArrays;

        public UpdateLightPacket()
	    {

	    }

	    public override void Decode(MinecraftStream stream)
	    {
		    ChunkX = stream.ReadVarInt();
		    ChunkZ = stream.ReadVarInt();
		    TrustEdges = stream.ReadBool();
		    SkyLightMask = stream.ReadVarInt();
		    BlockLightMask = stream.ReadVarInt();
		    EmptySkyLightMask = stream.ReadVarInt();
		    EmptyBlockLightMask = stream.ReadVarInt();

		  //  List<byte[]> skyLightList = new List<byte[]>();
			SkyLightArrays = new byte[16][];
		    for(int i = 0; i < SkyLightArrays.Length + 1; i++)
		    {
			    if (((SkyLightMask & 1) != 0) /*&& ((EmptySkyLightMask & 1) == 0)*/)
			    {
				    stream.ReadVarInt();
					byte[] data = stream.Read(2048);

					if (i != 0)
					{
						SkyLightArrays[i - 1] = data;
					}
			    }
			    else
			    {
				    if (i != 0)
				    {
					    SkyLightArrays[i - 1] = null;
				    }
			    }

			    SkyLightMask = SkyLightMask >> 1;
			    EmptySkyLightMask = EmptySkyLightMask >> 1;
            }
            //SkyLightArrays = skyLightList.ToArray();

            // List<byte[]> blockLightList = new List<byte[]>();
		    BlockLightArrays = new byte[16][];
            for (int i = 0; i < BlockLightArrays.Length + 1; i++)
		    {
			    if (((BlockLightMask & 1) != 0) /*&& ((EmptyBlockLightMask & 1) == 0)*/)
			    {
				    stream.ReadVarInt();
				    byte[] data = stream.Read(2048);

				    if (i != 0)
				    {
					    BlockLightArrays[i - 1] = data;
				    }
			    }
			    else
			    {
				    if (i != 0)
				    {
					    BlockLightArrays[i - 1] = null;
				    }
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
