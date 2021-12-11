using System;
using System.Linq;
using System.Threading.Tasks;
using Alex.Blocks.Storage;
using Alex.Networking.Java.Util;
using Alex.Worlds.Chunks;
using fNbt;
using fNbt.Tags;
using NLog;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaChunkColumn : ChunkColumn
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		/// <inheritdoc />
		public JavaChunkColumn(int x, int z) : base(x, z) { }
		public JavaChunkColumn(int x, int z, WorldSettings worldSettings) : base(x, z, worldSettings) { }
		
		protected override ChunkSection CreateSection(bool storeSkylight, int sections)
		{
			return new JavaChunkSection(storeSkylight, sections);
		}
		
		public void Read(MinecraftStream ms, NbtCompound heightmaps, bool readSkylight)
		{
			if (!heightmaps.TryGet("MOTION_BLOCKING", out NbtLongArray longArray))
			{
				Log.Warn($"Invalid chunk data!");
				return;
			}

			//
			var dataArray = longArray.Value;
			var realHeight = WorldSettings.WorldHeight + Math.Abs(WorldSettings.MinY);
			var bitsPerBlock = (int)Math.Ceiling(Math.Log2(realHeight + 1));//, 256
			
			uint maxHeight = uint.MinValue;
			FlexibleStorage flexibleStorage = new FlexibleStorage(bitsPerBlock, 256);
			var valueMask = (uint) ((1L << bitsPerBlock) - 1);
			int bitOffset = 0;
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					if (64 - (bitOffset % 64) < bitsPerBlock)
					{
						bitOffset += 64 - (bitOffset % 64);
					}

					int startLongIndex = bitOffset / 64;
					int end_long_index = startLongIndex;
					int startOffset = bitOffset % 64;
					bitOffset += bitsPerBlock;

					if (startLongIndex >= dataArray.Length || end_long_index >= dataArray.Length)
						continue;
                            
					uint rawId;

					if (startLongIndex == end_long_index)
					{
						rawId = (uint) (dataArray[startLongIndex] >> startOffset);
					}
					else
					{
						int endOffset = 64 - startOffset;

						rawId = (uint) (dataArray[startLongIndex] >> startOffset
						                | dataArray[end_long_index] << endOffset);
					}

					rawId &= valueMask;

					flexibleStorage[x + z * 16] = rawId;
				}
			}
			//flexibleStorage.SetData(longArray.Value);

			for (int i = 0; i < flexibleStorage.Length; i++)
			{
				maxHeight = Math.Max(maxHeight, flexibleStorage[i]);
			}

			var sections = (int)Math.Ceiling((maxHeight / 16D));// + 1;

			try
			{
				for (int sectionY = 0; sectionY < Math.Min(this.Sections.Length, sections); sectionY++)
				{
					var storage = this.Sections[sectionY];

					if (storage == null)
					{
						storage = CreateSection(readSkylight, 2);
					}

					if (storage is JavaChunkSection jcs)
					{
						jcs.Read(ms);
					}

					this.Sections[sectionY] = storage;
				}

				for (int i = 0; i < Sections.Length; i++)
				{
					Sections[i]?.RemoveInvalidBlocks();
				}

				for (int x = 0; x < 16; x++)
				{
					for (int z = 0; z < 16; z++)
					{
						for (int y = WorldSettings.MinY; y < WorldSettings.WorldHeight; y++)
						{
							SetScheduled(x, y, z, true);
						}
					}
				}
				
				CalculateHeight();
			}
			catch (Exception e)
			{
				Log.Warn($"Received supposedly corrupted chunk:" + e);
			}
		}
	}
}