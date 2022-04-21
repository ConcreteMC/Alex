using System;
using System.Collections.Generic;
using System.IO;
using Alex.Blocks.Storage;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Chunks;
using fNbt;
using MiNET.Net;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockChunkColumn : ChunkColumn
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockChunkColumn));
		/// <inheritdoc />
		public BedrockChunkColumn(int x, int z, WorldSettings worldSettings) : base(x, z, worldSettings) { }

		/// <inheritdoc />
		public BedrockChunkColumn(int x, int z) : base(x, z) { }

		/// <inheritdoc />
		protected override ChunkSection CreateSection(bool storeSkylight, int storages)
		{
			return new BedrockChunkSection(storages);
		}

		public static BedrockChunkColumn ReadFrom(ChunkProcessor processor, byte[] data, int x, int z, uint subChunkCount, WorldSettings worldSettings)
		{
			var column = new BedrockChunkColumn(x, z, worldSettings);

			using (MemoryStream ms = new MemoryStream(data))
			{
				if (subChunkCount > 0)
				{
					for (int i = 0; i < subChunkCount; i++)
					{
						int index = i;

						if (ms.Position >= ms.Length)
						{
							Log.Warn($"Not enough data! SubchunkCount={i} Count={subChunkCount}");

							break;
						}

						BedrockChunkSection section = BedrockChunkSection.Read(
							processor, ms, ref index, worldSettings);

						column[index] = section;
					}
				}

				if (ms.Position >= ms.Length)
					return column;

				BiomeStorage last = null;
				for (int i = 0; i < 24; i++)
				{
					if (ms.Position >= ms.Length)
						return column;
					
					var biomeStorage = DecodePalettedBiomeStorage(processor, ms);

					if (biomeStorage == null)
					{
						if (i == 0)
							return column;

						biomeStorage = last;
					}

					if (biomeStorage != null)
					{
						column.BiomeStorages[i] = biomeStorage;
						last = biomeStorage;
					}
				}
				
				if (ms.Position >= ms.Length)
					return column;
				
				int borderBlock = (byte)ms.ReadByte();

				if (borderBlock != 0)
				{
					for (int i = 0; i < borderBlock; i++)
					{
						ms.ReadByte();
					}
				}

				foreach (var blockEntity in ReadBlockEntities(ms))
				{
					if ((blockEntity.TryGet("x", out NbtInt xTag) || blockEntity.TryGet("X", out xTag))
					    && (blockEntity.TryGet("y", out NbtInt yTag) || blockEntity.TryGet("Y", out yTag))
					    && (blockEntity.TryGet("z", out NbtInt zTag) || blockEntity.TryGet("Z", out zTag)))
					{
						column.AddBlockEntity(new BlockCoordinates(xTag.Value, yTag.Value, zTag.Value), blockEntity);
					}
				}

				if (ms.Position < ms.Length)
				{
					int len = (int) (ms.Length - ms.Position);
					var bytes = new byte[len];
					ms.Read(bytes, 0, len);
					Log.Warn($"Still have data to read\n{Packet.HexDump(new ReadOnlyMemory<byte>(bytes))}");
				}
			}

			return column;
		}
		
		private static IEnumerable<NbtCompound> ReadBlockEntities(Stream stream)
		{
			while (stream.Position < stream.Length)
			{
				var b = stream.ReadByte();

				if (b == 255)
				{
					break;
				}

				stream.Position -= 1;

				NbtCompound compound = null;
				try
				{
					NbtFile file = new NbtFile() {BigEndian = false, UseVarInt = true};

					file.LoadFromStream(stream, NbtCompression.None);
					var blockEntityTag = file.RootTag;

					if (blockEntityTag is NbtCompound nbtCompound)
					{
						compound = nbtCompound;
						//chunkColumn.AddBlockEntity(new BlockCoordinates(x, y, z), nbtCompound);
					}
					else
					{
						Log.Info($"Got non-compound tag! Type={blockEntityTag.TagType} Name={blockEntityTag.Name}");
					}
				}
				catch (EndOfStreamException) { }
				catch(NbtFormatException){}

				if (compound != null)
					yield return compound;
				// if (Log.IsTraceEnabled()) Log.Trace($"Blockentity:\n{file.RootTag}");
			}
		}

		private static BiomeStorage DecodePalettedBiomeStorage(ChunkProcessor processor, Stream stream)
		{
			var blockSize = (byte)stream.ReadByte();
			bool isRuntime = (blockSize & 1) != 0;

			var bitsPerBlock = (byte) (blockSize >> 1);

			if (blockSize == 0x7f || bitsPerBlock == 0)
				return null;

			var words = ChunkProcessingUtils.ReadWordArray(stream, bitsPerBlock);

			if (words == null || words.Length == 0)
				return null;

			var palette = ChunkProcessingUtils.ReadPalette(stream, bitsPerBlock, new BiomePaletteEncoding());

			if (palette == null)
				return null;


			BiomeStorage biomeStorage = new BiomeStorage(8, 4096, 16, 16, 16);
			biomeStorage.MaxBitsPerEntry = 8;
			
			var blocksPerWord = (int) Math.Ceiling(4096f / words.Length);

			int position = 0;
			foreach (var word in words)
			{
				for (uint block = 0; block < blocksPerWord; block++)
				{
					if (position >= 4096)
						continue;

					var state = (uint) ((word >> ((position % blocksPerWord) * bitsPerBlock)) & ((1 << bitsPerBlock) - 1));

					if (state < palette.Length)
					{
						var runtimeId = palette[state];

						if (runtimeId != 0)
						{
							int x = (position >> 8) & 0xF;
							int y = position & 0xF;
							int z = (position >> 4) & 0xF;

							var blockState =
								processor.GetBiome(runtimeId); // processor.GetBlockState((uint) runtimeId);

							if (blockState != null)
							{
								biomeStorage.Set(x, y, z, blockState);
							}
						}
					}

					position++;
				}
			}

			return biomeStorage;
		}
	}
}