using System;
using System.Collections;
using System.Collections.Generic;
using Alex.Networking.Java.Util;
using fNbt;

namespace Alex.Networking.Java.Packets.Play
{
	public class ChunkDataPacket : Packet<ChunkDataPacket>
	{
		public int ChunkX;
		public int ChunkZ;
		public long[] PrimaryBitmask;
		public Memory<byte> Buffer;
		public List<NbtCompound> TileEntities;
		public NbtCompound HeightMaps;
		//public bool GroundUp;
	//	public bool IgnoreOldData;
		public int[] Biomes;

		public ChunkDataPacket()
		{
			PacketId = 0x22;
			TileEntities = new List<NbtCompound>();
		}

	
		public override void Decode(MinecraftStream stream)
		{
			ChunkX = stream.ReadInt();
			ChunkZ = stream.ReadInt();
			var bitmaskLength = stream.ReadVarInt();
			var bitmasks = new long[bitmaskLength];

			for (int ind = 0; ind < bitmasks.Length; ind++)
			{
				bitmasks[ind] = stream.ReadLong();
			}

			PrimaryBitmask = bitmasks;
			//GroundUp = stream.ReadBool();
			//IgnoreOldData = stream.ReadBool();
			//PrimaryBitmask = stream.ReadVarInt();

			HeightMaps = stream.ReadNbtCompound();

			//if (GroundUp)
			{
				int biomeCount = stream.ReadVarInt();
				
				int[] biomeIds = new int[biomeCount];
				for (int idx = 0; idx < biomeIds.Length; idx++)
				{
					biomeIds[idx] = stream.ReadVarInt();
				}

				Biomes = biomeIds;
			}

			int i = stream.ReadVarInt();
			Buffer = new Memory<byte>(new byte[i]);
			stream.Read(Buffer.Span, Buffer.Length);
			
			int tileEntities = stream.ReadVarInt();
			for (int k = 0; k < tileEntities; k++)
			{
				TileEntities.Add(stream.ReadNbtCompound());
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteInt(ChunkX);
			stream.WriteInt(ChunkZ);
		//	stream.WriteBool(GroundUp);
			stream.WriteVarInt(PrimaryBitmask.Length);

			for (int i = 0; i < PrimaryBitmask.Length; i++)
			{
				stream.WriteLong(PrimaryBitmask[i]);
			}

			//stream.WriteVarInt(PrimaryBitmask);
			stream.WriteVarInt(Buffer.Length);
			stream.Write(Buffer, 0, Buffer.Length);
			stream.WriteVarInt(TileEntities.Count);
			foreach (var tileEntity in TileEntities)
			{
				stream.WriteNbtCompound(tileEntity);
			}
		}

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();

			Biomes = null;
			HeightMaps = null;
			Buffer = null;
			TileEntities = new List<NbtCompound>();
		}
	}
}
