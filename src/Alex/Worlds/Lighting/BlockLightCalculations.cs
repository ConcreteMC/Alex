using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using NLog;

namespace Alex.Worlds.Lighting
{
    public class BlockLightCalculations
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger(typeof(BlockLightCalculations));
        private static ConcurrentQueue<(BlockCoordinates coords, Func<bool> action)> Queue { get; } = new ConcurrentQueue<(BlockCoordinates coords, Func<bool> action)>();
		private static ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<BlockCoordinates>> ChunkQueues { get; } = new ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<BlockCoordinates>>();
		
        private World World { get; }
        public BlockLightCalculations(World level)
        {
	        World = level;
        }
        
        public void Calculate(World level, BlockCoordinates blockCoordinates)
		{
			//Interlocked.Add(ref touches, 1);

			var lightBfsQueue = ChunkQueues.GetOrAdd((ChunkCoordinates) blockCoordinates,
				coordinates => new ConcurrentQueue<BlockCoordinates>());
			//ConcurrentQueue<BlockCoordinates> lightBfsQueue = new ConcurrentQueue<BlockCoordinates>();

			lightBfsQueue.Enqueue(blockCoordinates);
		/*	while (lightBfsQueue.TryDequeue(out var coords))
			{
				ProcessNode(level, coords, lightBfsQueue);
			}*/
		}

        public bool HasEnqueued(ChunkCoordinates coordinates)
        {
	        if (ChunkQueues.TryGetValue(coordinates, out var queue))
	        {
		        return !queue.IsEmpty;
	        }

	        return false;
        }
        
        public void Process(ChunkCoordinates coordinates)
        {
	        if (ChunkQueues.TryGetValue(coordinates, out var queue))
	        {
		        while (queue.TryDequeue(out var coords))
		        {
			        ProcessNode(World, coords, queue);
		        }

		        ChunkQueues.TryRemove(coordinates, out _);
	        }
        }

        public void Enqueue(BlockCoordinates coordinates)
        {
	       /* if (!Queue.Contains(coordinates))
	        {
		        Queue.Enqueue(coordinates);*/

		        ChunkQueues.AddOrUpdate((ChunkCoordinates) coordinates,
			        chunkCoordinates =>
			        {
				        var newQueue = new ConcurrentQueue<BlockCoordinates>();
				        newQueue.Enqueue(coordinates);

				        return newQueue;
			        },
			        (chunkCoordinates, queue) =>
			        {
				        if (!queue.Contains(coordinates))
				        {
					        queue.Enqueue(coordinates);
				        }

				        return queue;
			        });
	       // }
        }
        
        public bool TryProcess(Func<BlockCoordinates, bool> canProcess, out BlockCoordinates processedCoordinates)
        {
	        processedCoordinates = default;
	        if (Queue.TryDequeue(out var queued))
	        {
		        processedCoordinates = queued.coords;
		        if (canProcess.Invoke(processedCoordinates))
		        {
			        var result = queued.action?.Invoke();
			        if (result.HasValue && !result.Value)
			        {
				        Queue.Enqueue(queued);
				        return false;
			        }

			        if (!result.HasValue)
				        return false;

			        return true;
		        }

		        /*if (ChunkQueues.TryGetValue((ChunkCoordinates) coordinates, out var chunkQueue))
		        {
			        ProcessNode(World, coordinates, chunkQueue);
		        }*/

		        //return true;
	        }

	        return false;
        }
        
        private void ProcessNode(World level, BlockCoordinates coord, ConcurrentQueue<BlockCoordinates> lightBfsQueue)
		{
			//Log.Debug($"Setting light on block {block.Id} with LightLevel={block.LightLevel} and BlockLight={block.Blocklight}");
			ChunkColumn chunk = GetChunk(level, coord);
			if (chunk == null) return;

			int lightLevel = chunk.GetBlocklight(coord.X & 0x0f, coord.Y & 0xff, coord.Z & 0x0f);

			Test(level, coord, coord.BlockUp(), lightBfsQueue, chunk, lightLevel);
			Test(level, coord, coord.BlockDown(), lightBfsQueue, chunk, lightLevel);
			Test(level, coord, coord.BlockWest(), lightBfsQueue, chunk, lightLevel);
			Test(level, coord, coord.BlockEast(), lightBfsQueue, chunk, lightLevel);
			Test(level, coord, coord.BlockSouth(), lightBfsQueue, chunk, lightLevel);
			Test(level, coord, coord.BlockNorth(), lightBfsQueue, chunk, lightLevel);

			//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.Down, chunk), lightLevel);
			//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.West, chunk), lightLevel);
			//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.East, chunk), lightLevel);
			//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.South, chunk), lightLevel);
			//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.North, chunk), lightLevel);
		}

		private ChunkColumn GetChunk(World level, BlockCoordinates blockCoordinates)
		{
			return level.GetChunk(blockCoordinates);
		}

		public static long touches = 0;

		private void Test(World level, BlockCoordinates coord, BlockCoordinates newCoord, ConcurrentQueue<BlockCoordinates> lightBfsQueue, ChunkColumn chunk, int lightLevel)
		{
			//Interlocked.Add(ref touches, 1);

			bool isOtherChunk = false;
			var newChunkCoord = (ChunkCoordinates) newCoord;
			if (chunk.X != newChunkCoord.X || chunk.Z != newChunkCoord.Z)
			{
				chunk = GetChunk(level, newCoord);
				lightBfsQueue =
					ChunkQueues.GetOrAdd(newChunkCoord, coordinates => new ConcurrentQueue<BlockCoordinates>());
					
				isOtherChunk = true;
			}

			if (isOtherChunk && chunk == null)
			{
				Queue.Enqueue((newCoord, () =>
				{
					if (ChunkQueues.TryGetValue((ChunkCoordinates) newCoord, out var queue))
					{
						var cc = GetChunk(level, newCoord);
						if (cc == null)
							return false;

						var ll = level.GetBlockLight(coord);
						
						if (cc.GetBlock(newCoord.X & 0x0f, newCoord.Y & 0xff, newCoord.Z & 0x0f) is Air)
						{
							SetLightLevel(cc, queue, newCoord, ll);
						}
						else
						{
							SetLightLevel(level, cc, queue, newCoord, level.GetBlockLight(newCoord), (Block)level.GetBlock(newCoord), ll);
						}

						Enqueue(coord);

						return true;
					}

					return false;
				}));
				return;
			}

			if (chunk.GetBlock(newCoord.X & 0x0f, newCoord.Y & 0xff, newCoord.Z & 0x0f) is Air)
			{
				SetLightLevel(chunk, lightBfsQueue, newCoord, lightLevel);
			}
			else
			{
				SetLightLevel(level, chunk, lightBfsQueue, newCoord, level.GetBlockLight(newCoord),
					(Block) level.GetBlock(newCoord), lightLevel);
			}
		}

		private void SetLightLevel(World world, ChunkColumn chunk, ConcurrentQueue<BlockCoordinates> lightBfsQueue, BlockCoordinates coordinates, int currentBlockLight, Block block, int lightLevel)
		{
			if (currentBlockLight > 0)
			{
				if (currentBlockLight >= lightLevel)
				{
					return;
				}

				currentBlockLight = (byte) Math.Max(currentBlockLight, lightLevel - 1);
				chunk.SetBlocklight(coordinates.X & 0x0f, coordinates.Y & 0xff, coordinates.Z & 0x0f, (byte) currentBlockLight);
			}

			if ((!block.Solid || block.Transparent) && currentBlockLight + 2 <= lightLevel)
			{
				currentBlockLight = (byte) (lightLevel - 1);
				chunk.SetBlocklight(coordinates.X & 0x0f, coordinates.Y & 0xff, coordinates.Z & 0x0f, (byte) currentBlockLight);
				lightBfsQueue.Enqueue(coordinates);
			}
		}

		private void SetLightLevel(ChunkColumn chunk, ConcurrentQueue<BlockCoordinates> lightBfsQueue, BlockCoordinates coord, int lightLevel)
		{
			if (chunk.GetBlocklight(coord.X & 0x0f, coord.Y & 0xff, coord.Z & 0x0f) + 2 <= lightLevel)
			{
				chunk.SetBlocklight(coord.X & 0x0f, coord.Y & 0xff, coord.Z & 0x0f, (byte) (lightLevel - 1));
				lightBfsQueue.Enqueue(coord);
			}
		}
    }
}