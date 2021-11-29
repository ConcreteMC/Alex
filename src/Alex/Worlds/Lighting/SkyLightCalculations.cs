using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.Blocks.Minecraft;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.Common.Utils.Vectors.BlockCoordinates;
using ChunkColumn = Alex.Worlds.Chunks.ChunkColumn;
using ChunkCoordinates = Alex.Common.Utils.Vectors.ChunkCoordinates;
using Color = System.Drawing.Color;
using IBlockAccess = Alex.Worlds.Abstraction.IBlockAccess;

namespace Alex.Worlds.Lighting
{
	public class SkyLightCalculations : IDisposable
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger(typeof(SkyLightCalculations));
		
		public  bool              DoLogging         { get; set; } = false;
		private CancellationToken CancellationToken { get; }
		private IBlockAccess Level { get; }
		private ConcurrentQueue<BlockCoordinates> Queue { get; } = new ConcurrentQueue<BlockCoordinates>();
		public SkyLightCalculations(IBlockAccess level, CancellationToken cancellationToken)
		{
			Level = level;
			CancellationToken = cancellationToken;
		}

		public bool Recalculate(ChunkColumn chunk)
		{
			if (chunk == null) return false;

			var lightBfQueue = new Queue<BlockCoordinates>();
			var lightBfSet = new HashSet<BlockCoordinates>();
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
				//	if (!IsOnChunkBorder(x, z))
				//	{
				//		continue;
				//	}

					int height = GetHeighestSurrounding(x, z, chunk);
					if (height == 0)
					{
						continue;
					}

					//var skyLight = chunk.GetSkylight(x, height, z);
					//if (skyLight == 15)
					{
						//Block block = level.GetBlockId(new BlockCoordinates(x + (chunk.x*16), height, z + (chunk.z*16)), chunk);
						//Calculate(level, block);
						//Calculate(level, new BlockCoordinates(x + (chunk.x*16), height, z + (chunk.z*16)), lightBfQueue);
						var coordinates = new BlockCoordinates(x + (chunk.X * 16), height, z + (chunk.Z * 16));
						lightBfQueue.Enqueue(coordinates);
						lightBfSet.Add(coordinates);
					}
					//else
					//{
					//	Log.Error($"Block with wrong light level. Expected 15 but was {skyLight}");
					//}
				}
			}

			Calculate(lightBfQueue, lightBfSet);

			return true;
		}
		
		public int Execute()
		{
			int count = 0;

			while (Queue.TryDequeue(out var coords) && !CancellationToken.IsCancellationRequested)
			{
				count += Calculate(coords);
			}

			return count;
		}

		public void Enqueue(BlockCoordinates coordinates)
		{
			Queue.Enqueue(coordinates);
		}
		
		public int Calculate(BlockCoordinates coordinates)
		{
			var level = Level;

			if (level == null)
				return 0;
			
			int currentLight = level.GetSkyLight(coordinates);

			var cc = new ChunkCoordinates(coordinates);
			ChunkColumn chunk = (ChunkColumn) level.GetChunk(cc);

			if (chunk == null)
				return 0;
			
			var height = chunk.GetRecalculatedHeight(coordinates.X & 0x0f, coordinates.Z & 0x0f);

			Queue<BlockCoordinates> sourceQueue = new Queue<BlockCoordinates>();
			sourceQueue.Enqueue(coordinates);
			if (currentLight != 0)
			{
				Queue<BlockCoordinates> resetQueue = new Queue<BlockCoordinates>();
				HashSet<BlockCoordinates> visits = new HashSet<BlockCoordinates>();

				// Reset all lights that potentially derive from this
				resetQueue.Enqueue(coordinates);

				Queue<BlockCoordinates> deleteQueue = new Queue<BlockCoordinates>();
				while (resetQueue.Count > 0)
				{
					var coord = resetQueue.Dequeue();
					if (visits.Contains(coord))
					{
						continue;
					}

					visits.Add(coord);

					if (coord.DistanceTo(coordinates) > 16) continue;

					ResetLight(resetQueue, sourceQueue, coord);
					if (!sourceQueue.Contains(coord))
					{
						deleteQueue.Enqueue(coord);
					}
				}

				level.SetSkyLight(coordinates, 0);

				foreach (var delete in deleteQueue)
				{
					level.SetSkyLight(delete, 0);
				}
			}
			else
			{
				sourceQueue.Enqueue(coordinates);
				sourceQueue.Enqueue(coordinates.BlockUp());
				sourceQueue.Enqueue(coordinates.BlockDown());
				sourceQueue.Enqueue(coordinates.BlockWest());
				sourceQueue.Enqueue(coordinates.BlockEast());
				sourceQueue.Enqueue(coordinates.BlockNorth());
				sourceQueue.Enqueue(coordinates.BlockSouth());
			}

			chunk.SetHeight(coordinates.X & 0x0f, coordinates.Z & 0x0f, (short) height);

			// Recalc
			Queue<BlockCoordinates> lightBfQueue = new Queue<BlockCoordinates>(sourceQueue);
			HashSet<BlockCoordinates> lightBfSet = new HashSet<BlockCoordinates>(sourceQueue);

		//	SkyLightBlockAccess blockAccess = new SkyLightBlockAccess(level.ChunkManager);
			Calculate(lightBfQueue, lightBfSet);

			return 1;
		}

		private void ResetLight(Queue<BlockCoordinates> resetQueue, Queue<BlockCoordinates> sourceQueue, BlockCoordinates coordinates)
		{
			int currentLight = Level.GetSkyLight(coordinates);

			if (coordinates.Y < 255)
				TestForSource(resetQueue, sourceQueue, coordinates.BlockUp(), currentLight);
			if (coordinates.Y > 0)
				TestForSource(resetQueue, sourceQueue, coordinates.BlockDown(), currentLight, true);
			TestForSource(resetQueue, sourceQueue, coordinates.BlockWest(), currentLight);
			TestForSource( resetQueue, sourceQueue, coordinates.BlockEast(), currentLight);
			TestForSource( resetQueue, sourceQueue, coordinates.BlockNorth(), currentLight);
			TestForSource( resetQueue, sourceQueue, coordinates.BlockSouth(), currentLight);
		}

		private void TestForSource(Queue<BlockCoordinates> resetQueue, Queue<BlockCoordinates> sourceQueue, BlockCoordinates coordinates, int currentLight, bool down = false)
		{
			int light = Level.GetSkyLight(coordinates);
			if (light == 0) return;

			if (light > currentLight || (light == 15 && !down))
			{
				if (!sourceQueue.Contains(coordinates)) sourceQueue.Enqueue(coordinates);
				return;
			}

			if (!resetQueue.Contains(coordinates)) resetQueue.Enqueue(coordinates);
		}

		private void Calculate(Queue<BlockCoordinates> lightBfQueue, HashSet<BlockCoordinates> lightBfSet)
		{
			try
			{
				//if (block.SkyLight != 15)
				//{
				//	Log.Error($"Block at {block.Coordinates} had unexpected light level. Expected 15 but was {block.SkyLight}");
				//}

				while (lightBfQueue.Count > 0 && !CancellationToken.IsCancellationRequested)
				{
					var coordinates = lightBfQueue.Dequeue();
					lightBfSet.Remove(coordinates);

					ChunkColumn chunk = Level.GetChunk(coordinates);

					if (chunk == null)
					{
						if (DoLogging)
							Log.Warn($"Chunk was null");
						continue;
					}
					
					if (coordinates.Y < chunk.WorldSettings.MinY || coordinates.Y > chunk.WorldSettings.WorldHeight -1)
					{
						if (DoLogging)
							Log.Warn($"Y coord out of bounce {coordinates.Y}");
						continue;
					}

					var newChunkCoord = (ChunkCoordinates) coordinates;
					if (chunk.X != newChunkCoord.X || chunk.Z != newChunkCoord.Z)
					{
						chunk = (ChunkColumn) Level.GetChunk(newChunkCoord);
						if (chunk == null)
						{
							if (DoLogging)
								Log.Warn($"Chunk with new coords was null");
							continue;
						}
					}

					ProcessNode(chunk, coordinates, lightBfQueue, lightBfSet);
				}
			}
			catch (Exception e)
			{
				Log.Error(e, "Calculation");
			}
		}

		private void ProcessNode(ChunkColumn chunk, BlockCoordinates coordinates, Queue<BlockCoordinates> lightBfsQueue, HashSet<BlockCoordinates> lightBfSet)
		{
			//if (section.IsAllAir())

			byte currentSkyLight = GetSkyLight(coordinates, chunk);

			int sectionIdx = coordinates.Y >> 4;

			byte maxSkyLight = currentSkyLight;
			if (coordinates.Y < chunk.WorldSettings.WorldHeight - 1)
			{
				var up = coordinates.BlockUp();
				maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(chunk, sectionIdx, lightBfsQueue, lightBfSet, up, currentSkyLight, up: true));
			}

			if (coordinates.Y > chunk.WorldSettings.MinY)
			{
				var down = coordinates.BlockDown();
				maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(chunk, sectionIdx, lightBfsQueue, lightBfSet, down, currentSkyLight, down: true));
			}

			var west = coordinates.BlockWest();
			maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(chunk, sectionIdx, lightBfsQueue, lightBfSet, west, currentSkyLight));


			var east = coordinates.BlockEast();
			maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(chunk, sectionIdx, lightBfsQueue, lightBfSet, east, currentSkyLight));


			var south = coordinates.BlockSouth();
			maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(chunk, sectionIdx, lightBfsQueue, lightBfSet, south, currentSkyLight));

			var north = coordinates.BlockNorth();
			maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(chunk, sectionIdx, lightBfsQueue, lightBfSet, north, currentSkyLight));

			if (IsTransparent(coordinates, chunk) && currentSkyLight != 15)
			{
				int diffuseLevel = GetDiffuseLevel(coordinates, chunk);
				maxSkyLight = (byte) Math.Max(currentSkyLight, maxSkyLight - diffuseLevel);

				if (maxSkyLight > currentSkyLight)
				{
					Level.SetSkyLight(coordinates, maxSkyLight);

					if (!lightBfSet.Contains(coordinates))
					{
						lightBfsQueue.Enqueue(coordinates);
						lightBfSet.Add(coordinates);
					}
				}
			}
		}

		private byte SetLightLevel(ChunkColumn chunk, int sectionIdx, Queue<BlockCoordinates> lightBfsQueue, HashSet<BlockCoordinates> lightBfSet, BlockCoordinates coordinates, byte lightLevel, bool down = false, bool up = false)
		{
			var chunkCoords = (ChunkCoordinates) coordinates;
			
			if (!(up || down) && (chunk.X != coordinates.X >> 4 || chunk.Z != coordinates.Z >> 4))
			{
				chunk = (ChunkColumn) Level.GetChunk(chunkCoords);
			}
			else
			{
				if ((up || down) && coordinates.Y >> 4 != sectionIdx)
				{
					
				}
			}

			if (chunk == null /* || chunk.chunks == null*/)
			{
				return lightLevel;
			}

			if (!down && !up && coordinates.Y >= GetHeight(coordinates, chunk))
			{
				if (GetSkyLight(coordinates, chunk) != 15)
				{
					SetSkyLight(coordinates, 15);

					if (!lightBfSet.Contains(coordinates))
					{
						lightBfsQueue.Enqueue(coordinates);
						lightBfSet.Add(coordinates);
					}
				}

				return 15;
			}

			//if (subChunk == null) subChunk = (ChunkSection) chunk.Sections[coordinates.Y >> 4];

			bool isTransparent = IsTransparent(coordinates, chunk);
			byte skyLight = GetSkyLight(coordinates, chunk);

			if (down && isTransparent && lightLevel == 15)
			{
				if (IsNotBlockingSkylight(coordinates, chunk))
				{
					if (skyLight != 15)
					{
						SetSkyLight(coordinates, 15);
					}

					if (!lightBfSet.Contains(coordinates))
					{
						lightBfsQueue.Enqueue(coordinates);
						lightBfSet.Add(coordinates);
					}

					return 15;
				}
			}

			if (isTransparent)
			{
				int diffuseLevel = GetDiffuseLevel(coordinates, chunk);
				if (skyLight + 1 + diffuseLevel <= lightLevel)
				{
					byte newLevel = (byte) (lightLevel - diffuseLevel);
					SetSkyLight(coordinates, newLevel);

					if (!lightBfSet.Contains(coordinates))
					{
						lightBfsQueue.Enqueue(coordinates);
						lightBfSet.Add(coordinates);
					}

					return newLevel;
				}
			}

			return skyLight;
		}

		private void SetSkyLight(BlockCoordinates coordinates, byte skyLight)
		{
			Level.SetSkyLight(coordinates, skyLight);
			//chunk?.SetSkyLight(coordinates.X & 0x0f, coordinates.Y & 0xff, coordinates.Z & 0x0f, skyLight);
		}

		private static bool IsNotBlockingSkylight(BlockCoordinates blockCoordinates, ChunkColumn chunk)
		{
			if (chunk == null) return true;

			var b = chunk.GetBlockState(blockCoordinates.X & 0x0f, blockCoordinates.Y, blockCoordinates.Z & 0x0f).Block;
			return b == null || b is Air || !b.BlockMaterial.BlocksLight;// (b.Transparent && !(b is Leaves));
			//	int bid = chunk.GetBlockId(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0xff, blockCoordinates.Z & 0x0f);
			//	return bid == 0 || (BlockFactory.TransparentBlocks[bid] == 1 && bid != 18 && bid != 161 && bid != 30 && bid != 8 && bid != 9);
		}

		private static int GetDiffuseLevel(BlockCoordinates blockCoordinates, ChunkColumn section)
		{
			//TODO: Figure out if this is really correct. Perhaps should be zero.
			if (section == null) return 15;

			int bx = blockCoordinates.X & 0x0f;
			int by = blockCoordinates.Y;
			int bz = blockCoordinates.Z & 0x0f;

			var state = section.GetBlockState(bx, by, bz);
			return state?.Block?.Diffusion ?? 1;
			//return bid == 8 || bid == 9 ? 3 : bid == 18 || bid == 161 || bid == 30 ? 2 : 1;
		}

		private static bool IsTransparent(BlockCoordinates blockCoordinates, ChunkColumn section)
		{
			if (section == null) return true;

			int bx = blockCoordinates.X & 0x0f;
			int by = blockCoordinates.Y;
			int bz = blockCoordinates.Z & 0x0f;

			var state = section?.GetBlockState(bx, by, bz);
			return state == null || state.Block is Air || state.Block.Transparent;
		//	return bid == 0 || BlockFactory.TransparentBlocks[bid] == 1;
		}

		private static byte GetSkyLight(BlockCoordinates blockCoordinates, ChunkColumn chunk)
		{
			if (chunk == null) return 15;

			int bx = blockCoordinates.X & 0x0f;
			int by = blockCoordinates.Y;
			int bz = blockCoordinates.Z & 0x0f;

			return chunk.GetSkylight(bx, by, bz);
		}

		/*private static byte GetSkyLight(BlockCoordinates blockCoordinates, ChunkColumn chunk)
		{
			if (chunk == null) return 15;

			return chunk.GetSkylight(blockCoordinates.X & 0x0f, blockCoordinates.Y, blockCoordinates.Z & 0x0f);
		}*/

		private static int GetHeight(BlockCoordinates blockCoordinates, ChunkColumn chunk)
		{
			if (chunk == null) return 255;

			return chunk.GetHeight(blockCoordinates.X & 0x0f, blockCoordinates.Z & 0x0f);
		}
		
		private static bool IsOnChunkBorder(int x, int z)
		{
			return !(x > 0 && x < 15 && z > 0 && z < 15);
		}

		private int GetHeighestSurrounding(int x, int z, ChunkColumn chunk)
		{
			int h = chunk.GetHeight(x, z);
			if (h == 255) return h;

			if (x == 0 || x == 15 || z == 0 || z == 15)
			{
				var coords = new BlockCoordinates(x + (chunk.X * 16), h, z + (chunk.Z * 16));

				//h = Math.Max(h, level.GetHeight(coords + BlockCoordinates.Up));
				h = Math.Max(h, Level.GetHeight(coords + BlockCoordinates.West));
				h = Math.Max(h, Level.GetHeight(coords + BlockCoordinates.East));
				h = Math.Max(h, Level.GetHeight(coords + BlockCoordinates.North));
				h = Math.Max(h, Level.GetHeight(coords + BlockCoordinates.South));
				if (h > 255) h = 255;
				if (h < 0) h = 0;
				return h;
			}

			h = Math.Max(h, chunk.GetHeight(x, z + 1));
			h = Math.Max(h, chunk.GetHeight(x, z - 1));
			h = Math.Max(h, chunk.GetHeight(x + 1, z));
			h = Math.Max(h, chunk.GetHeight(x - 1, z));

			return h;
		}

		/// <inheritdoc />
		public void Dispose() { }
	}

}