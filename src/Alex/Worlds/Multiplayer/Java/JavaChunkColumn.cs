using System;
using Alex.Networking.Java.Util;
using Alex.Worlds.Chunks;
using NLog;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaChunkColumn : ChunkColumn
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		protected override ChunkSection CreateSection(int y, bool storeSkylight, int sections)
		{
			return new JavaChunkSection(this, y, storeSkylight, sections);
		}

		public void Read(MinecraftStream ms, int availableSections, bool groundUp, bool readSkylight)
		{
			try
			{
				//	Stopwatch s = Stopwatch.StartNew();
				//	Log.Debug($"Reading chunk data...");

				for (int sectionY = 0; sectionY < this.Sections.Length; sectionY++)
				{
					var storage = (JavaChunkSection) this.Sections[sectionY];
					if ((availableSections & (1 << sectionY)) != 0)
					{
						if (storage == null)
						{
							storage = new JavaChunkSection(this, sectionY, readSkylight);
						}

						storage.Read(ms);
					}
					else
					{
						if (groundUp && (storage == null || storage.Blocks > 0))
						{
							//if (storage == null)
							//	storage = new ChunkSection(this, sectionY, readSkylight, 2);
						}
					}

					if (storage != null)
						storage.IsDirty = true;

					this.Sections[sectionY] = storage;
				}

				if (groundUp)
				{
					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							var biomeId = ms.ReadInt();
							SetBiome(x, z, biomeId);
						}
					}
				}

				for (int i = 0; i < Sections.Length; i++)
				{
					Sections[i]?.RemoveInvalidBlocks();
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