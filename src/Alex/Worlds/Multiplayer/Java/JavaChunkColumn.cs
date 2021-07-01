using System;
using Alex.Networking.Java.Util;
using Alex.Worlds.Chunks;
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
		
		public static bool GetBitX(long[] data, int x) {
			var index = x/64;
			var bit = x-index*64;

			if (data.Length == 0 || index > data.Length - 1)
				return false;
			
			return (data[index] & (1<<bit)) != 0;
		}
		
		public void Read(MinecraftStream ms, long[] primaryBitMask, bool readSkylight)
		{
			
			try
			{
				//	Stopwatch s = Stopwatch.StartNew();
				//	Log.Debug($"Reading chunk data...");

				for (int sectionY = 0; sectionY < this.Sections.Length; sectionY++)
				{
					if (!GetBitX(primaryBitMask, sectionY))
						continue;

					var storage = (JavaChunkSection) this.Sections[sectionY];
					if (storage == null)
					{ 
						storage = new JavaChunkSection(readSkylight);
					}

					storage.Read(ms);

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