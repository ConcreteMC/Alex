using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

	
		public override async Task DecodeAsync(MinecraftStream stream)
		{
			ChunkX = await stream.ReadIntAsync();
			ChunkZ = await stream.ReadIntAsync();
			var bitmaskLength = await stream.ReadVarIntAsync();
			var bitmasks = new long[bitmaskLength];

			for (int ind = 0; ind < bitmasks.Length; ind++)
			{
				bitmasks[ind] = await stream.ReadLongAsync();
			}

			PrimaryBitmask = bitmasks;
			//GroundUp = stream.ReadBool();
			//IgnoreOldData = stream.ReadBool();
			//PrimaryBitmask = stream.ReadVarInt();

			HeightMaps = await stream.ReadNbtCompoundAsync();

			//if (GroundUp)
			{
				int biomeCount = await stream.ReadVarIntAsync();
				
				int[] biomeIds = new int[biomeCount];
				for (int idx = 0; idx < biomeIds.Length; idx++)
				{
					biomeIds[idx] = await stream.ReadVarIntAsync();
				}

				Biomes = biomeIds;
			}

			int i = await stream.ReadVarIntAsync();
			Buffer = new Memory<byte>(new byte[i]);
			await stream.ReadAsync(Buffer);
			
			int tileEntities = await stream.ReadVarIntAsync();
			for (int k = 0; k < tileEntities; k++)
			{
				TileEntities.Add(await stream.ReadNbtCompoundAsync());
			}
		}

		public override async Task EncodeAsync(MinecraftStream stream)
		{
			await stream.WriteIntAsync(ChunkX);
			await stream.WriteIntAsync(ChunkZ);
		//	stream.WriteBool(GroundUp);
			await stream.WriteVarIntAsync(PrimaryBitmask.Length);

			for (int i = 0; i < PrimaryBitmask.Length; i++)
			{
				await stream.WriteLongAsync(PrimaryBitmask[i]);
			}

			//stream.WriteVarInt(PrimaryBitmask);
			await stream.WriteVarIntAsync(Buffer.Length);
			await stream.WriteAsync(Buffer);
			await stream.WriteVarIntAsync(TileEntities.Count);
			foreach (var tileEntity in TileEntities)
			{
				await stream.WriteNbtCompoundAsync(tileEntity);
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
