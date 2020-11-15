using System.Collections.Concurrent;
using System.Collections.Generic;
using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;

namespace Alex.Worlds.Lighting
{
	public class SkyLightCalculator
	{
		private IBlockAccess                                                              World       { get; }
		private IChunkManager ChunkManager { get; }
		private ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingItem>> ChunkQueues { get; }
		//private ConcurrentQueue<LightingItem> LightingQueue { get; }
		public SkyLightCalculator(IBlockAccess blockAccess, IChunkManager chunkManager)
		{
			World = blockAccess;
				ChunkQueues = new ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingItem>>();
				//LightingQueue = new ConcurrentQueue<LightingItem>();
			ChunkManager = chunkManager;
		}

		public bool TryProcess(ChunkCoordinates coordinates)
		{
			if (ChunkQueues.TryGetValue(coordinates, out var queue) && ChunkManager.TryGetChunk(coordinates, out var chunk))
			{
				while (queue.TryDequeue(out var lightingItem))
				{
					Spread(queue, chunk, lightingItem.Face, lightingItem.Coordinates.X, lightingItem.Coordinates.Y,
						lightingItem.Coordinates.Z, lightingItem.LightLevel);
				}
				
				if (queue.IsEmpty)
					ChunkQueues.TryRemove(coordinates, out _);

				return true;
			}

			return false;
		}

		public void Calculate(ChunkColumn chunk, ChunkCoordinates chunkCoordinates)
		{
			var cx = (chunkCoordinates.X << 4);
			var cz = (chunkCoordinates.Z << 4);
			for (int x = 0; x < 16; x++)
			{
				var rx = cx + x;
				for (int z = 0; z < 16; z++)
				{
					var rz     = cz + z;
					int height = chunk.GetHeight(x, z);

					for (int y = height; y < 255; y++)
					{
						Process(chunk, rx, height, rz);
						//Process(chunk, x, y, z);
					}
				}
			}
		}

		private void Process(ChunkColumn originalChunk, int x, int y, int z)
		{
			Enqueue(BlockFace.West, x - 1, y, z, 15);
			Enqueue(BlockFace.East, x + 1, y, z, 15);
			Enqueue(BlockFace.North, x, y, z - 1, 15);
			Enqueue(BlockFace.South, x, y, z + 1, 15);
			
			if (y > 0)
				Enqueue(BlockFace.Down, x, y - 1, z, 15);
			
			if (y < ChunkColumn.ChunkHeight)
				Enqueue(BlockFace.Up, x, y + 1, z, 15);
		}

		private void Spread(ConcurrentQueue<LightingItem> queue, ChunkColumn chunk, BlockFace face, int x, int y, int z, int lightLevel)
		{
			if (lightLevel <= 0)
				return;

			var chunkCoordinates = new ChunkCoordinates(x >> 4, z >> 4);

			if (chunkCoordinates.X != chunk.X || chunkCoordinates.Z != chunk.Z)
			{
				Enqueue(face, x, y, z, lightLevel);
				return;
			}

			//if (!ChunkManager.TryGetChunk(chunkCoordinates, out var chunk))
			//{
			//	Enqueue(face, x, y, z, lightLevel);
			//	return;
			//}

			//var self = World.GetBlockState(x, y, z).Block;
			var self = chunk.GetBlockState(x & 0xf, y & 0xff, z & 0xf).Block;

			if (self.BlockMaterial.BlocksLight)
				return;
			
			lightLevel -= self.LightOpacity;

			if (lightLevel < 0) 
				lightLevel = 0;

			if (lightLevel < chunk.GetSkylight(x & 0xf, y & 0xff, z & 0xf))
				return;

			chunk.SetSkyLight(x & 0xf, y & 0xff, z & 0xf, (byte) lightLevel);

			if (lightLevel <= 0)
				return;

			//lightLevel--;

			if (face != BlockFace.East)
			{
				Enqueue(queue,  BlockFace.West, x - 1, y, z, lightLevel);
			}

			if (face != BlockFace.West)
			{
				Enqueue(queue, BlockFace.East, x + 1, y, z, lightLevel);
			}

			if (face != BlockFace.South)
			{
				Enqueue(queue, 
					BlockFace.North, x, y, z - 1, lightLevel);
			}

			if (face != BlockFace.North)
			{
				Enqueue(queue, BlockFace.South, x, y, z + 1, lightLevel);
			}

			if (face != BlockFace.Up && y > 0)
			{
				Enqueue(queue, BlockFace.Down, x, y - 1, z, lightLevel);
			}

			if (face != BlockFace.Down && y < 256)
			{
				Enqueue(queue, BlockFace.Up, x, y + 1, z, lightLevel);
			}
		}

		private void Enqueue(ConcurrentQueue<LightingItem> queue, BlockFace face, int x, int y, int z, int lightLevel)
		{
			queue.Enqueue(new LightingItem(face, new BlockCoordinates(x, y, z), lightLevel));
		}
		
		private void Enqueue(BlockFace face, int x, int y, int z, int lightLevel)
		{
			var chunkQueue = ChunkQueues.GetOrAdd(
				new ChunkCoordinates(x >> 4, z >> 4), coordinates => new ConcurrentQueue<LightingItem>());
			
			chunkQueue.Enqueue(new LightingItem(face, new BlockCoordinates(x, y, z), lightLevel));
		}

		private class LightingItem
		{
			public BlockFace        Face        { get; set; }
			public BlockCoordinates Coordinates { get; set; }
			public int              LightLevel  { get; set; }

			public LightingItem(BlockFace blockFace, BlockCoordinates coordinates, int lightLevel)
			{
				Face = blockFace;
				Coordinates = coordinates;
				LightLevel = lightLevel;
			}
		}
	}
}