using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Alex.Networking.Java.Util;
using fNbt;

namespace Alex.Networking.Java.Packets.Play
{
	public class ChunkDataPacket : Packet<ChunkDataPacket>
	{
		public int ChunkX;
		public int ChunkZ;
		
		public Memory<byte> Buffer;
		public List<BlockEntityData> TileEntities;
		public NbtCompound HeightMaps;

		public BitSet SkyLightMask;

		public BitSet BlockLightMask;

		public BitSet EmptySkyLightMask;

		public BitSet EmptyBlockLightMask;

		public byte[][] SkyLight;
		public byte[][] BlockLight;
		
		public ChunkDataPacket()
		{
			PacketId = 0x22;
			TileEntities = new List<BlockEntityData>();
		}

	
		public override async Task DecodeAsync(MinecraftStream stream)
		{
			ChunkX = await stream.ReadIntAsync();
			ChunkZ = await stream.ReadIntAsync();
			
			HeightMaps = await stream.ReadNbtCompoundAsync();

			int i = await stream.ReadVarIntAsync();
			Buffer = new Memory<byte>(new byte[i]);
			await stream.ReadAsync(Buffer);
			
			int tileEntities = await stream.ReadVarIntAsync();
			for (int k = 0; k < tileEntities; k++)
			{
				BlockEntityData blockEntity = new BlockEntityData();
				await blockEntity.ReadAsync(stream);
				TileEntities.Add(blockEntity);
			}

			bool trustEdges = await stream.ReadBoolAsync();
			SkyLightMask = await BitSet.ReadAsync(stream);
			BlockLightMask = await BitSet.ReadAsync(stream);
			EmptySkyLightMask = await BitSet.ReadAsync(stream);
			EmptyBlockLightMask = await BitSet.ReadAsync(stream);

			int skyLightArrayCount = await stream.ReadVarIntAsync();
			SkyLight = new byte[skyLightArrayCount][];
			for (int idx = 0; idx < SkyLight.Length; idx++)
			{
				int length = await stream.ReadVarIntAsync();
				SkyLight[idx] = await stream.ReadAsync(length);
			}
			
			int blockLightArrayCount = await stream.ReadVarIntAsync();
			BlockLight = new byte[blockLightArrayCount][];
			for (int idx = 0; idx < BlockLight.Length; idx++)
			{
				int length = await stream.ReadVarIntAsync();
				BlockLight[idx] = await stream.ReadAsync(length);
			}
		}

		public override async Task EncodeAsync(MinecraftStream stream)
		{
			await stream.WriteIntAsync(ChunkX);
			await stream.WriteIntAsync(ChunkZ);
		//	stream.WriteBool(GroundUp);
			/*await stream.WriteVarIntAsync(PrimaryBitmask.Length);

			for (int i = 0; i < PrimaryBitmask.Length; i++)
			{
				await stream.WriteLongAsync(PrimaryBitmask[i]);
			}*/

			//stream.WriteVarInt(PrimaryBitmask);
			await stream.WriteVarIntAsync(Buffer.Length);
			await stream.WriteAsync(Buffer);
			await stream.WriteVarIntAsync(TileEntities.Count);
			foreach (var tileEntity in TileEntities)
			{
				await tileEntity.WriteAsync(stream);
			}
		}

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();

			//Biomes = null;
			HeightMaps = null;
			Buffer = null;
			TileEntities = new List<BlockEntityData>();
		}

		public class BlockEntityData
		{
			public byte X { get; set; }
			public byte Z { get; set; }
			public short Y { get; set; }
			public int Type { get; set; }
			public NbtCompound Data { get; set; }
			public BlockEntityData()
			{
				
			}

			public async Task ReadAsync(MinecraftStream stream)
			{
				var packedXZ = await stream.ReadUnsignedByteAsync();
				
				X = (byte)(packedXZ >> 4);
				Z = (byte)((packedXZ) & 15);
				
				Y = await stream.ReadShortAsync();
				Type = await stream.ReadVarIntAsync();
				Data = await stream.ReadNbtCompoundAsync();
			}

			public async Task WriteAsync(MinecraftStream stream)
			{
				
			}
		}
	}
}
