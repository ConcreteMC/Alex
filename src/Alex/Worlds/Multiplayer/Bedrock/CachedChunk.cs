using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.Common.Utils.Vectors;
using Alex.Net.Bedrock;
using Alex.Worlds.Chunks;
using fNbt;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class CachedChunk
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(CachedChunk));

		public int X { get; }
		public int Z { get; }
		public ulong[] SubChunks { get; set; } = new ulong[16];
		public byte[][] Sections { get; set; } = new byte[16][];
		public ulong Biome { get; set; }

		public ChunkColumn Chunk { get; }

		public bool IsComplete => Sections.All(x => x != null);
		// public Action<ChunkColumn> Callback { get; set; }
		//public uint               SubChunkCount = 0;

		public CachedChunk(int x, int z)
		{
			X = x;
			Z = z;

			Chunk = new ChunkColumn(x, z);
			Chunk.IsNew = true;
		}

		public void TryBuild(BedrockClient client, ChunkProcessor processor)
		{
			bool complete = IsComplete;

			if (!complete)
			{
				complete = true;

				for (int i = 0; i < Sections.Length; i++)
				{
					if (Sections[i] != null)
						continue;

					if (processor.Cache.TryGet(SubChunks[i], out var data))
					{
						Sections[i] = data;
					}
					else
					{
						complete = false;
					}
				}
			}

			if (!complete || !processor.Cache.TryGet(Biome, out var biomeIds))
				return;

			if (biomeIds.Length >= 256)
			{
				for (int x = 0; x < 16; x++)
				{
					for (int z = 0; z < 16; z++)
					{
						var biomeId = biomeIds[(z << 4) + (x)];

						for (int y = 0; y < 255; y++)
						{
							Chunk.SetBiome(x, y, z, Worlds.BiomeUtils.GetBiome(biomeId));
						}
					}
				}
			}

			processor._futureChunks.Remove(this);

			var coordinates = new ChunkCoordinates(Chunk.X, Chunk.Z);

			foreach (KeyValuePair<BlockCoordinates, NbtCompound> bePair in processor._futureBlockEntities.Where(
				         be => (ChunkCoordinates)be.Key == coordinates))
			{
				Chunk.AddBlockEntity(bePair.Key, bePair.Value);
				processor._futureBlockEntities.TryRemove(bePair.Key, out _);
			}

			for (int i = 0; i < Sections.Length; i++)
			{
				var sectionData = Sections[i];

				if (sectionData == null)
				{
					if (processor.Cache.TryGet(SubChunks[i], out sectionData))
					{
						Sections[i] = sectionData;
					}
					else
					{
						Log.Warn($"Missing data for subchunk");
					}
				}

				if (sectionData != null && sectionData.Length > 0)
				{
					var sectionIndex = i;
					using (MemoryStream ms = new MemoryStream(sectionData))
					{
						using NbtBinaryReader defStream = new NbtBinaryReader(ms, true);
						var section = processor.ReadSection(defStream, out var sectionYIndex);

						if (sectionYIndex != int.MaxValue)
						{
							sectionIndex = sectionYIndex;
						}
						
						Chunk.Sections[sectionIndex] = section;
					}
				}
			}

			Chunk.CalculateHeight();

			client.World.ChunkManager.AddChunk(Chunk, coordinates, true);
		}
	}
}