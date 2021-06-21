using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Blocks.Minecraft;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Chunks;
using NLog;

namespace Alex.Worlds.Lighting
{
    public class BlockLightCalculations : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger(typeof(BlockLightCalculations));
        private static ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<BlockCoordinates>> ChunkQueues { get; } = new ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<BlockCoordinates>>();
		
        private World             World             { get; }
        private CancellationToken CancellationToken { get; }
        public BlockLightCalculations(World level, CancellationToken cancellationToken)
        {
	        World = level;
	        CancellationToken = cancellationToken;
        }

        public void Remove(ChunkCoordinates coordinates)
        {
	        ChunkQueues.TryRemove(coordinates, out _);
        }

        public void Clear()
        {
	        ChunkQueues.Clear();
	      //  Queue.Clear();
        }
        
        public bool HasEnqueued(ChunkCoordinates coordinates)
        {
	        if (ChunkQueues.TryGetValue(coordinates, out var queue))
	        {
		        return !queue.IsEmpty;
	        }

	        return false;
        }
        
        public void Recalculate(ChunkColumn chunk)
        {
	        ChunkQueues.TryRemove(new ChunkCoordinates(chunk.X, chunk.Z), out _);
	        //  var lightSources = chunk.GetLightSources().ToArray();

	        foreach (var section in chunk.Sections)
	        {
		        section?.ResetLight(true, false);
		        section?.RemoveInvalidBlocks();
	        }
	        
	        var chunkpos = new BlockCoordinates(chunk.X << 4, 0, chunk.Z << 4);
	        foreach (var lightsource in chunk.GetLightSources().OrderByDescending(x => x.Y))
	        {
		        Calculate(chunkpos + lightsource);
	        }
	        //chunk.GetLightSources()
        }
        
        public bool Process(ChunkCoordinates coordinates)
        {
	        int count = 0;
	        if (ChunkQueues.TryGetValue(coordinates, out var queue))
	        {
		        while (queue.TryDequeue(out var coords) && !CancellationToken.IsCancellationRequested)
		        {
			        ProcessNode(World, coords, queue);
			        count++;
		        }

		        if (queue.IsEmpty)
		        {
			        ChunkQueues.TryRemove(coordinates, out _);
		        }
	        }

	        return count > 0;
        }

        public void Calculate(BlockCoordinates coordinates)
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

        private void ProcessNode(World level, BlockCoordinates coord, ConcurrentQueue<BlockCoordinates> lightBfsQueue)
		{
			if (level.TryGetBlockLight(coord, out var lightLevel))
			{
				Test(level, coord, coord + BlockCoordinates.Up, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.Down, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.North, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.East, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.South, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.West, lightBfsQueue, lightLevel);
				
				/*Test(level, coord, coord + BlockCoordinates.Up, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.Left, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.Right, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.Forwards, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.Backwards, lightBfsQueue, lightLevel);
				Test(level, coord, coord + BlockCoordinates.Down, lightBfsQueue, lightLevel);*/
				
				//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.Down, chunk), lightLevel);
				//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.West, chunk), lightLevel);
				//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.East, chunk), lightLevel);
				//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.South, chunk), lightLevel);
				//SetLightLevel(level, lightBfsQueue, level.GetBlockId(coord + BlockCoordinates.North, chunk), lightLevel);
			}
		}

		private ChunkColumn GetChunk(World level, BlockCoordinates blockCoordinates)
		{
			return level.GetChunk(blockCoordinates);
		}

		private void Test(World level, BlockCoordinates sourceBlock, BlockCoordinates target, ConcurrentQueue<BlockCoordinates> lightBfsQueue, int lightLevel)
		{
			var chunkCoord = new ChunkCoordinates(sourceBlock);
			//Interlocked.Add(ref touches, 1);

			bool isOtherChunk = false;
			var newChunkCoord = (ChunkCoordinates) target;
			if (chunkCoord.X != newChunkCoord.X || chunkCoord.Z != newChunkCoord.Z)
			{
				//chunk = GetChunk(level, newCoord);
				lightBfsQueue =
					ChunkQueues.GetOrAdd(newChunkCoord, coordinates => new ConcurrentQueue<BlockCoordinates>());
					
				isOtherChunk = true;
			}

			if (isOtherChunk && !World.TryGetBlockLight(target, out _))
			{
				lightBfsQueue.Enqueue(target);
				/*Queue.Enqueue((newCoord, () =>
				{
					if (ChunkQueues.TryGetValue((ChunkCoordinates) newCoord, out var queue))
					{
						if (!level.TryGetBlockLight(coord, out var ll))
						{
							return false;
						}
						//var cc = GetChunk(level, newCoord);
						//if (cc == null)
						//	return false;
						//var ll = level.GetBlockLight(coord);
						
						DoPass(level, newCoord, queue, ll);
						
						Enqueue(coord);
						Enqueue(newCoord);
						return true;
					}
					return false;
				}));*/
				
				return;
			}

			DoPass(level, target, lightBfsQueue, lightLevel);
		}

		private void DoPass(World level, BlockCoordinates target,
			ConcurrentQueue<BlockCoordinates> lightBfsQueue, int lightLevel)
		{
			var block = level.GetBlockState(target).Block;// chunk.GetBlockState(newCoord.X & 0x0f, newCoord.Y & 0xff, newCoord.Z & 0x0f).Block;

			if (!block.Renderable || block.BlockMaterial == Material.Air) 
			{
				SetLightLevel(lightBfsQueue, target, lightLevel);
			}
			else
			{
				SetLightLevel(level, lightBfsQueue, target, level.GetBlockLight(target),
					(Block) block, lightLevel);
			}
		}

		private void UpdateNeighbors(World world, BlockCoordinates coordinates)
		{
			//var source = new BlockCoordinates(x, y, z);

			world.ScheduleBlockUpdate(new BlockCoordinates(coordinates.X + 1, coordinates.Y, coordinates.Z));
			world.ScheduleBlockUpdate(new BlockCoordinates(coordinates.X + -1, coordinates.Y, coordinates.Z));

			world.ScheduleBlockUpdate(new BlockCoordinates(coordinates.X, coordinates.Y, coordinates.Z + 1));
			world.ScheduleBlockUpdate(new BlockCoordinates(coordinates.X, coordinates.Y, coordinates.Z + -1));

			world.ScheduleBlockUpdate(new BlockCoordinates(coordinates.X, coordinates.Y + 1, coordinates.Z));
			world.ScheduleBlockUpdate(new BlockCoordinates(coordinates.X, coordinates.Y + -1, coordinates.Z));
		}
		
		private void SetLightLevel(World world, ConcurrentQueue<BlockCoordinates> lightBfsQueue, BlockCoordinates coordinates, int currentLightLevel, Block block, int lightLevel)
		{
			if (currentLightLevel > 0)
			{
				if (currentLightLevel >= lightLevel)
				{
					return;
				}
				
				world.SetBlockLight(coordinates, (byte) Math.Max(currentLightLevel, lightLevel - 1));
				
				//UpdateNeighbors(world, coordinates);
				
				return;
			}

			if ((!block.Solid || block.Transparent) && currentLightLevel + 2 <= lightLevel)
			{
				world.SetBlockLight(coordinates,  (byte) (lightLevel - 1));
				
				//UpdateNeighbors(world, coordinates);
				
			//	if (!lightBfsQueue.Contains(coordinates))
				{
					lightBfsQueue.Enqueue(coordinates);
				}
			}
		}

		private void SetLightLevel(ConcurrentQueue<BlockCoordinates> lightBfsQueue, BlockCoordinates coord, int lightLevel)
		{
			if (World.GetBlockLight(coord) + 2 <= lightLevel)
			{
				//chunk.SetBlocklight(coord.X & 0x0f, coord.Y & 0xff, coord.Z & 0x0f, (byte) (lightLevel - 1));
				World.SetBlockLight(coord, (byte) (lightLevel - 1));
				
			//	UpdateNeighbors(World, coord);
				
				//if (!lightBfsQueue.Contains(coord))
				{
					lightBfsQueue.Enqueue(coord);
				}
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			//Queue.Clear();
			ChunkQueues.Clear();
		}
    }
}