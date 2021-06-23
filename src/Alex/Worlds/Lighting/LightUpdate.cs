using System;
using System.Collections.Generic;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;

namespace Alex.Worlds.Lighting
{
	public abstract class LightUpdate
	{
		internal static readonly BlockCoordinates[] Adjacents = new[]
		{
			new BlockCoordinates(1, 0, 0),
			new BlockCoordinates(-1, 0, 0),
			new BlockCoordinates(0, 1, 0),
			new BlockCoordinates(0, -1, 0),
			new BlockCoordinates(0, 0, 1),
			new BlockCoordinates(0, 0, -1),
		};
		protected SubChunkExplorer SubChunkExplorer { get; }
		private Dictionary<int, LightUpdateNode> UpdateNodes { get; }
		private object _nodeLock = new object();
		public LightUpdate(SubChunkExplorer chunkExplorer)
		{
			SubChunkExplorer = chunkExplorer;
			UpdateNodes = new Dictionary<int, LightUpdateNode>();
		}

		protected abstract LightArray GetCurrentLightArray();

		public abstract void RecalculateNode(int x, int y, int z);
		
		public abstract int RecalculateChunk(int chunkX, int chunkZ);

		private LightPropagationContext PrepareNodes()
		{
			var context = new LightPropagationContext();

			lock (_nodeLock)
			{
				foreach (var blockHash in UpdateNodes.Values)
				{
					if (SubChunkExplorer.MoveTo(blockHash.X, blockHash.Y, blockHash.Z)
					    != SubChunkExplorer.ChunkExplorerStatus.Invalid)
					{
						var lightArray = GetCurrentLightArray();
						var oldLevel = lightArray.Get(blockHash.X & 0xf, blockHash.Y & 0xf, blockHash.Z & 0xf);

						if (oldLevel != blockHash.LightValue)
						{
							lightArray.Set(
								blockHash.X & 0xf, blockHash.Y & 0xf, blockHash.Z & 0xf, blockHash.LightValue);

							var index = World.BlockHash(blockHash.X, blockHash.Y, blockHash.Z);

							if (oldLevel < blockHash.LightValue)
							{
								context.SpreadVisited[index] = true;

								context.SpreadQueue.Enqueue(
									new BlockCoordinates(blockHash.X, blockHash.Y, blockHash.Z));
							}
							else
							{
								context.RemovalVisited[index] = true;

								context.RemovalQueue.Enqueue(
									new LightUpdateNode(blockHash.X, blockHash.Y, blockHash.Z, oldLevel));
							}
						}
					}
				}
			}

			return context;
		}

		protected byte GetEffectiveLight(int x, int y, int z) {
			if(this.SubChunkExplorer.MoveTo(x, y, z) != SubChunkExplorer.ChunkExplorerStatus.Invalid){
				return GetCurrentLightArray().Get(x & 0xf, y & 0xf, z & 0xf);
			}
			
			return 0;
		}
		
		protected byte GetHighestAdjacentLight(int x, int y, int z) {
			byte adjacent = 0;
			foreach(var adj in Adjacents){
				if((adjacent = Math.Max(adjacent, GetEffectiveLight(x + adj.X, y + adj.Y, z + adj.Z))) == 15){
					break;
				}
			}
			
			return adjacent;
		}
		
		public void SetAndUpdateLight(int x, int y, int z, byte newLevel) {
			UpdateNodes[World.BlockHash(x, y, z)] = new LightUpdateNode(x, y, z, newLevel);
		}

		public int Execute()
		{
			var context = PrepareNodes();
			int touched = 0;

			while (context.RemovalQueue.TryDequeue(out var dequeued))
			{
				touched++;

				foreach (var adj in Adjacents)
				{
					var cx = dequeued.X + adj.X;
					var cy = dequeued.Y + adj.Y;
					var cz = dequeued.Z + adj.Z;

					var idx = World.BlockHash(cx, cy, cz);

					if (this.SubChunkExplorer.MoveTo(cx, cy, cz) != SubChunkExplorer.ChunkExplorerStatus.Invalid)
					{
						ComputeRemoveLight(cx, cy, cz, dequeued.LightValue, context);
					}
					else if (GetEffectiveLight(cx, cy, cz) > 0
					         && !context.SpreadVisited.ContainsKey(idx))
					{
						context.SpreadVisited[idx] = true;
						context.SpreadQueue.Enqueue(new BlockCoordinates(cx, cy, cz));
					}
				}
			}

			while (context.SpreadQueue.TryDequeue(out var dequeued))
			{
				touched++;

				var sv = World.BlockHash(dequeued.X, dequeued.Y, dequeued.Z);
				
				if (context.SpreadVisited.ContainsKey(sv))
					context.SpreadVisited.Remove(sv);

				var newAdjacentLight = GetEffectiveLight(dequeued.X, dequeued.Y, dequeued.Z);

				if (newAdjacentLight <= 0)
				{
					continue;
				}
				
				foreach (var adj in Adjacents)
				{
					var cx = dequeued.X + adj.X;
					var cy = dequeued.Y + adj.Y;
					var cz = dequeued.Z + adj.Z;

					var idx = World.BlockHash(cx, cy, cz);

					if (this.SubChunkExplorer.MoveTo(cx, cy, cz) != SubChunkExplorer.ChunkExplorerStatus.Invalid)
					{
						ComputeSpreadLight(cx, cy, cz, newAdjacentLight, context);
					}
				}
			}

			/*foreach (var visited in context.SpreadVisited)
			{
				if (!visited.Value)
					continue;

				var blockcoords = World.BlockHashDecode(visited.Key);

				if (SubChunkExplorer.MoveTo(blockcoords.X, blockcoords.Y, blockcoords.Z)
				    != SubChunkExplorer.ChunkExplorerStatus.Invalid)
				{
					var chunk = SubChunkExplorer.CurrentChunk;
					if (chunk == null) 
						continue;

					chunk.ScheduleBlockUpdate(blockcoords.X & 0xf, blockcoords.Y & 0xf, blockcoords.Z & 0xf);
				}
			}
			
			foreach (var visited in context.RemovalVisited)
			{
				if (!visited.Value)
					continue;

				var blockcoords = World.BlockHashDecode(visited.Key);

				if (SubChunkExplorer.MoveTo(blockcoords.X, blockcoords.Y, blockcoords.Z)
				    != SubChunkExplorer.ChunkExplorerStatus.Invalid)
				{
					var chunk = SubChunkExplorer.CurrentChunk;
					if (chunk == null) 
						continue;

					chunk.ScheduleBlockUpdate(blockcoords.X & 0xf, blockcoords.Y & 0xf, blockcoords.Z & 0xf);
				}
			}*/
			//while(!)

			return touched;
		}

		protected void ComputeSpreadLight(int x, int y, int z, int newAdjacentLevel, LightPropagationContext context)
		{
			var lightArray = GetCurrentLightArray();
			var	current = lightArray.Get(x & 0xf, y & 0xf, z & 0xf);
			var potentialLight = newAdjacentLevel - 1;

			if(current < potentialLight){
				lightArray.Set(x & 0xf, y & 0xf, z & 0xf, (byte) potentialLight);
				
				var idx = World.BlockHash(x, y, z);
				if(!context.SpreadVisited.ContainsKey(idx) && potentialLight > 1){
					context.SpreadVisited[idx] = true;
					context.SpreadQueue.Enqueue(new BlockCoordinates(x,y,z));
				}
			}
		}

		protected void ComputeRemoveLight(int x, int y, int z, int oldAdjacentLevel, LightPropagationContext context){
			var lightArray = GetCurrentLightArray();
			var	current = lightArray.Get(x & 0xf, y & 0xf, z & 0xf);

			if(current != 0 && current < oldAdjacentLevel){
				lightArray.Set(x & 0xf, y & 0xf, z & 0xf, 0);

				var index = World.BlockHash(x, y, z);

					//if (!isset(context.RemovalVisited[index]))
				if (!context.RemovalVisited.ContainsKey(index))
				{
					context.RemovalVisited[index] = true;

					if (current > 1)
					{
						context.RemovalQueue.Enqueue(new LightUpdateNode(x,y,z,current));
					}
				}
			}
			else if (current >= oldAdjacentLevel)
			{
				var index = World.BlockHash(x, y, z);

				if (!context.SpreadVisited.ContainsKey(index))
				{
					context.SpreadVisited[index] = true;
					context.SpreadQueue.Enqueue(new BlockCoordinates(x, y, z));
				}
			}
		}
	}

	public class LightUpdateNode
	{
		public int X { get; }
		public int Y { get; }
		public int Z { get; }
		public byte LightValue { get; }

		public LightUpdateNode(int x, int y, int z, byte light)
		{
			X = x;
			Y = y;
			Z = z;
			LightValue = light;
		}
	}
	
	public class LightPropagationContext
	{
		public Queue<BlockCoordinates> SpreadQueue { get; }
		public Queue<LightUpdateNode> RemovalQueue { get; }
		
		public Dictionary<int, bool> SpreadVisited { get; }
		public Dictionary<int, bool> RemovalVisited { get; }
		
		public LightPropagationContext()
		{
			SpreadQueue = new Queue<BlockCoordinates>();
			RemovalQueue = new Queue<LightUpdateNode>();
			
			SpreadVisited = new Dictionary<int, bool>();
			RemovalVisited = new Dictionary<int, bool>();
		}
	}
}