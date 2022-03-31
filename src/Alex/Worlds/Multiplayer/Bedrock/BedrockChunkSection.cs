using System;
using System.IO;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Blocks.Storage.Palette;
using Alex.Worlds.Chunks;
using fNbt;
using MiNET.Net;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockChunkSection : ChunkSection
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockChunkSection));
		
		public BedrockChunkSection(int storageCount) : base(storageCount)
		{
			
		}
		
		public BedrockChunkSection(BlockStorage[] blockStorages) : base(blockStorages)
		{
			
		}

		public static BedrockChunkSection Read(ChunkProcessor processor, Stream stream, ref int index, WorldSettings worldSettings)
		{
			var version = (byte)stream.ReadByte();

			BedrockChunkSection section = null;// = new BedrockChunkSection(0);
			
			int blockStorages = 1;
			switch (version)
			{
				case 1:
					section = ReadLegacyChunkSection(processor, stream);
					break;
				case 8:
				case 9:
					var storageCount = stream.ReadByte();

					if (version == 9)
					{
						var uIndex = stream.ReadByte();
						index = ((sbyte) uIndex) - (worldSettings.MinY >> 4);
					}

					BlockStorage[] storages = null;
					if (storageCount > 0)
					{
						storages = new BlockStorage[storageCount];
						
						if (storageCount > 2)
							Log.Info($"Storagecount exceeds expectations: {storageCount} ({(sbyte)storageCount})");
						
						for (int i = 0; i < storageCount; i++)
						{
							storages[i] = DecodePalettedBlockStorage(processor, stream);
						}
					}

					if (storages != null)
					{
						section = new BedrockChunkSection(storages);
					}

					break;
			}

			return section;
		}

		private static BlockStorage DecodePalettedBlockStorage(ChunkProcessor processor, Stream stream)
		{
			var blockSize = (byte)stream.ReadByte();
			bool isRuntime = (blockSize & 1) != 0;
			
			var bitsPerEntry = (byte)(blockSize >> 1);

			if (blockSize == 0x7f)
				return null;
			
			var dataArray = ChunkProcessingUtils.ReadWordArray(stream, bitsPerEntry);
			
			if (dataArray == null || dataArray.Length == 0)
				return null;

			var palette = ChunkProcessingUtils.ReadPalette(stream, bitsPerEntry, new BlockPaletteEncoding(isRuntime));

			if (palette == null)
				return null;
			
			var blocksPerWord = (int) Math.Ceiling(4096f / dataArray.Length);
			BlockStorage blockStorage = new BlockStorage();
			
			int position = 0;
			foreach (var word in dataArray)
			{
				if (position >= 4096)
					break;
				
				for (uint block = 0; block < blocksPerWord; block++)
				{
					if (position >= 4096)
						break;

					var state = (uint) ((word >> ((position % blocksPerWord) * bitsPerEntry))
					                    & ((1 << bitsPerEntry) - 1));
					
					if (state >= palette.Length)
						continue;

					var id = palette[state];
					
					int x = (position >> 8) & 0xF;
					int y = position & 0xF;
					int z = (position >> 4) & 0xF;

					blockStorage.Set(x,y,z, processor.GetBlockState(id));

					position++;
				}
			}

			return blockStorage;
		}

		private static BedrockChunkSection ReadLegacyChunkSection(ChunkProcessor processor, Stream defStream)
		{
			byte[] blockIds = new byte[4096];

			if (defStream.Read(blockIds, 0, blockIds.Length) != blockIds.Length)
				return null;

			NibbleArray data = new NibbleArray(4096);

			if (defStream.Read(data.Data, 0, data.Data.Length) != data.Data.Length)
				return null;

			var section = new BedrockChunkSection(1);

			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 16; y++)
					{
						int idx = (x << 8) + (z << 4) + y;
						var id = blockIds[idx];
						var meta = data[idx];

						//var ruid = BlockFactory.GetBlockStateID(id, meta);

						var block = MiNET.Blocks.BlockFactory.GetRuntimeId(id, meta);
						BlockState result = processor.GetBlockState(block);

						if (result != null)
						{
							section.Set(x, y, z, result);
						}
						else
						{
							Log.Info($"Unknown block: {id}:{meta}");
						}
					}
				}
			}

			return section;
		}
	}
}