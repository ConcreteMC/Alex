using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Alex.API.Utils;
using Alex.API.World;
using Microsoft.Xna.Framework;


namespace Alex.Worlds.Lighting
{
    //Taken from https://github.com/ddevault/TrueCraft/blob/master/TrueCraft.Core/Lighting/WorldLighting.cs
	//Modified to work with Alex

    public class SkylightCalculations
    {
	    private static readonly BlockCoordinates[] Neighbors =
	    {
		    BlockCoordinates.Up,
		    BlockCoordinates.Down,
		    BlockCoordinates.East,
		    BlockCoordinates.West,
		    BlockCoordinates.North,
		    BlockCoordinates.South
	    };

	    private struct LightingOperation
	    {
		    public BoundingBox Box { get; set; }
		    public bool SkyLight { get; set; }
		    public bool Initial { get; set; }
		    public ChunkCoordinates Coordinates { get; set; }
	    }

	    public int HighPriorityPending => HighPriorityQueue.Sum(x => x.Value.Count);
	    public int MidPriorityPending => MidPriorityQueue.Sum(x => x.Value.Count);
	    public int LowPriorityPending => LowPriorityQueue.Sum(x => x.Value.Count);

        public AutoResetEvent ResetEvent { get; }
		private ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>> HighPriorityQueue { get; set; }
	    private ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>> MidPriorityQueue { get; set; }
	    private ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>> LowPriorityQueue { get; set; }

        private ConcurrentDictionary<ChunkCoordinates, byte[,]> HeightMaps { get; set; }
        private IWorld World { get; }

        private Func<ChunkCoordinates, CheckResult> CheckDistance { get; }
        public SkylightCalculations(IWorld world, Func<ChunkCoordinates, CheckResult> distanceFunc)
        {
	        CheckDistance = distanceFunc;
		    World = world;

			HighPriorityQueue = new ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>>();
			MidPriorityQueue = new ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>>();
			LowPriorityQueue = new ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>>();

			ResetEvent = new AutoResetEvent(false);
		    HeightMaps = new ConcurrentDictionary<ChunkCoordinates, byte[,]>();
        }

	    private void GenerateHeightMap(IChunkColumn chunk)
	    {
		    BlockCoordinates coords;
		    var map = new byte[16 , 16];
		    for (byte x = 0; x < 16; x++)
		    {
			    for (byte z = 0; z < 16; z++)
			    {
				    for (byte y = (byte)(chunk.GetHeight(x, z) + 2); y > 0; y--)
				    {
					    if (y >= 255)
						    continue;
					    coords.X = x; coords.Y = y - 1; coords.Z = z;
					    
					    var provider = chunk.GetBlockState(coords.X, coords.Y, coords.Z).Block;

						if (!provider.Renderable) continue;

					    if (provider == null || provider.LightOpacity != 0)
					    {
						    map[x, z] = y;
						    break;
					    }
				    }
			    }
		    }
		    HeightMaps[new ChunkCoordinates(chunk.X, chunk.Z)] = map;
	    }

	    public void UpdateHeightMap(BlockCoordinates coords)
	    {
		    IChunkColumn chunk;
		    var adjusted = World.FindBlockPosition(coords, out chunk);

            if (chunk == null) return;
		    var chunkPos = new ChunkCoordinates(chunk.X, chunk.Z);

            if (!HeightMaps.ContainsKey(chunkPos))
			    return;
		    var map = HeightMaps[chunkPos];
		    byte x = (byte)adjusted.X; byte z = (byte)adjusted.Z;
		    BlockCoordinates _;
		    for (byte y = (byte)(chunk.GetHeight(x, z) + 2); y > 0; y--)
		    {
			    if (y >= 255)
				    continue;
			    _.X = x; _.Y = y - 1; _.Z = z;
			    var provider = chunk.GetBlockState(_.X, _.Y, _.Z).Block;

			    if (!provider.Renderable) continue;
                if (provider.LightOpacity != 0)
			    {
				    map[x, z] = y;
				    break;
			    }
		    }

		    
	    }

        private void LightBox(LightingOperation op)
	    {
		    var corners = op.Box.GetCorners();
		   // var center = op.Coordinates;
            ChunkCoordinates center = new ChunkCoordinates(new PlayerLocation(corners.Average(p => p.X), corners.Average(p => p.Y), corners.Average(p => p.Z)));
		    var chunk = World.GetChunkColumn(center.X, center.Z);
		    if (chunk == null)
			    return;
		   
		    for (int x = (int)op.Box.Min.X; x < (int)op.Box.Max.X; x++)
		    for (int z = (int)op.Box.Min.Z; z < (int)op.Box.Max.Z; z++)
		    for (int y = (int)op.Box.Max.Y - 1; y >= (int)op.Box.Min.Y; y--)
		    {
			    LightVoxel(x, y, z, op);
		    }

		    chunk.SkyLightDirty = false;
	    }

        private void PropegateLightEvent(int x, int y, int z, byte value, LightingOperation op)
	    {
			var coords = new BlockCoordinates(x,y,z);
		    if (!World.HasBlock(x,y,z))
			    return;

		    IChunkColumn chunk;
		    var adjustedCoords = World.FindBlockPosition(coords, out chunk);
		    if (chunk == null)
			    return;

            byte current = op.SkyLight ? chunk.GetSkylight(adjustedCoords.X, adjustedCoords.Y, adjustedCoords.Z) : chunk.GetBlocklight(adjustedCoords.X,adjustedCoords.Y, adjustedCoords.Z);
		    if (value == current)
			    return;

		    var provider = chunk.GetBlockState(adjustedCoords.X, adjustedCoords.Y, adjustedCoords.Z);
		    if (op.Initial)
		    {
			    byte emissiveness = (byte)provider.Block.LightValue;
			    if (chunk.GetHeight((byte)adjustedCoords.X, (byte)adjustedCoords.Z) <= y)
				    emissiveness = 15;
			    if (emissiveness >= current)
				    return;
		    }
		    EnqueueOperation(new ChunkCoordinates(chunk.X, chunk.Z),  new BoundingBox(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1)), op.SkyLight, op.Initial);
        }
        /// <summary>
        /// Computes the correct lighting value for a given voxel.
        /// </summary>
        private void LightVoxel(int x, int y, int z, LightingOperation op)
        {
            var coords = new BlockCoordinates(x, y, z);

            IChunkColumn chunk;
            var adjustedCoords = World.FindBlockPosition(coords, out chunk);

            if (chunk == null) // Move on if this chunk is empty
                return;

            var provider = chunk.GetBlockState(adjustedCoords.X, adjustedCoords.Y, adjustedCoords.Z).Block;
           // var provider = BlockRepository.GetBlockProvider(id);

            // The opacity of the block determines the amount of light it receives from
            // neighboring blocks. This is subtracted from the max of the neighboring
            // block values. We must subtract at least 1.
            byte opacity = (byte)Math.Max(provider.LightOpacity, (byte)1);

            byte current = op.SkyLight ? chunk.GetSkylight(adjustedCoords.X, adjustedCoords.Y, adjustedCoords.Z) : chunk.GetBlocklight(adjustedCoords.X, adjustedCoords.Y, adjustedCoords.Z);
            byte final = 0;

            // Calculate emissiveness
            byte emissiveness = (byte)provider.LightValue;
            if (op.SkyLight)
            {
	            var chunkPos = new ChunkCoordinates(chunk.X, chunk.Z);
                byte[,] map;
                if (!HeightMaps.TryGetValue(chunkPos, out map))
                {
                    GenerateHeightMap(chunk);
                    map = HeightMaps[chunkPos];
                }
	           // var height = chunk.GetHeight(adjustedCoords.X, adjustedCoords.Z);
                var height = map[adjustedCoords.X, adjustedCoords.Z];
                // For skylight, the emissiveness is 15 if y >= height
                if (y >= height)
                    emissiveness = 15;
                else
                {
                    if (provider.LightOpacity >= 15)
                        emissiveness = 0;
                }
            }

            if (opacity < 15 || emissiveness != 0)
            {
                // Compute the light based on the max of the neighbors
                byte max = 0;
                for (int i = 0; i < Neighbors.Length; i++)
                {
	                IChunkColumn c;
	                var adjusted = World.FindBlockPosition(coords + Neighbors[i], out c);
                    //var n = coords + Neighbors[i];
                    //  if (World.HasBlock(n.X, n.Y, n.Z))
	                if (c != null)
	                {
		                byte val;
		                if (op.SkyLight)
			                val = c.GetSkylight(adjusted.X, adjusted.Y, adjusted.Z);
		                else
			                val = c.GetBlocklight(adjusted.X, adjusted.Y, adjusted.Z);
		                max = Math.Max(max, val);
	                }
                }
                // final = MAX(max - opacity, emissiveness, 0)
                final = (byte)Math.Max(max - opacity, emissiveness);
                if (final < 0)
                    final = 0;
            }

            if (final != current)
            {
                // Apply changes
                if (op.SkyLight)
                    chunk.SetSkyLight(adjustedCoords.X, adjustedCoords.Y, adjustedCoords.Z, final);
                else
                    chunk.SetBlocklight(adjustedCoords.X, adjustedCoords.Y, adjustedCoords.Z, final);

                byte propegated = (byte)Math.Max(final - 1, 0);

                // Propegate lighting change to neighboring blocks
	            if (x - 1 <= op.Box.Min.X)
                    PropegateLightEvent(x - 1, y, z, propegated, op);
	            if (y - 1 <= op.Box.Min.Y)
                    PropegateLightEvent(x, y - 1, z, propegated, op);
	            if (z - 1 <= op.Box.Min.Z)
                    PropegateLightEvent(x, y, z - 1, propegated, op);

                if (x + 1 >= op.Box.Max.X)
                    PropegateLightEvent(x + 1, y, z, propegated, op);
                if (y + 1 >= op.Box.Max.Y)
                    PropegateLightEvent(x, y + 1, z, propegated, op);
                if (z + 1 >= op.Box.Max.Z)
                    PropegateLightEvent(x, y, z + 1, propegated, op);
            }
        }

	    public bool HasPending()
	    {
		    return HighPriorityQueue.Any(x => x.Value.Count > 0) || LowPriorityQueue.Any(x => x.Value.Count > 0) || MidPriorityQueue.Any(x => x.Value.Count > 0);
	    }

	    public enum CheckResult
	    {
			LowPriority,
			MediumPriority,
			HighPriority,
			Cancel
	    }

	    private void FixPriority(LightingOperation op, CheckResult result)
	    {
		    switch (result)
		    {
			    case CheckResult.LowPriority:
					Enqueue(LowPriorityQueue, op);
				    break;
			    case CheckResult.MediumPriority:
				    Enqueue(MidPriorityQueue, op);
                    break;
			    case CheckResult.HighPriority:
				    Enqueue(HighPriorityQueue, op);
                    break;
			    case CheckResult.Cancel:
				    break;
		    }
	    }

	    private void Enqueue(ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>> dict,
		    LightingOperation op)
	    {
		    var r = dict.GetOrAdd(op.Coordinates, coordinates => new ConcurrentQueue<LightingOperation>());
			r.Enqueue(op);
	    }

	    private bool TryDequeue(
		    ConcurrentDictionary<ChunkCoordinates, ConcurrentQueue<LightingOperation>> dict, ChunkCoordinates center, out LightingOperation op)
	    {
		    var f = dict.OrderBy(x => Math.Abs(x.Key.DistanceTo(center))).FirstOrDefault(x => !x.Value.IsEmpty);
		    if (f.Value == null)
		    {
			    op = default(LightingOperation);
			    return false;
		    }

		    return f.Value.TryDequeue(out op);
	    }

	    public void Remove(ChunkCoordinates c)
	    {
		    HighPriorityQueue.TryRemove(c, out _);
		    MidPriorityQueue.TryRemove(c, out _);
		    LowPriorityQueue.TryRemove(c, out _);
	    }

	    public bool TryLightNext(ChunkCoordinates ourCoordinates, out ChunkCoordinates coordinates)
	    {		    
		    coordinates = ChunkCoordinates.Zero;

		    if (!HasPending())
			    return false;

		    bool dequeued = false;

		    if (TryDequeue(HighPriorityQueue, ourCoordinates, out LightingOperation op))
		    {
			    dequeued = true;
		    }
		    else if (TryDequeue(MidPriorityQueue, ourCoordinates, out op))
		    {
			    dequeued = true;
		    }
		    else if (TryDequeue(LowPriorityQueue, ourCoordinates, out op))
		    {
			    dequeued = true;
		    }

		    if (dequeued)
		    {
			    var chunk = World.GetChunkColumn(op.Coordinates.X, op.Coordinates.Z);
			    if (chunk == null) return false;

			    LightBox(op);
			    coordinates = op.Coordinates;
			    return true;
		    }


		    return false;
	    }

	    public void EnqueueOperation(ChunkCoordinates coords, BoundingBox box, bool skyLight, bool initial = false)
	    {
		    var op = new LightingOperation
		    {
			    SkyLight = skyLight, Box = box, Initial = initial, Coordinates = coords
		    };

		    CheckResult result = CheckResult.HighPriority;
		    if (!op.Initial)
		    {
			    result = CheckDistance(coords);

			    if (result == CheckResult.Cancel)
			    {
				    return;
			    }
		    }

		    FixPriority(op, result);

            ResetEvent.Set();
	    }

	    private void SetUpperVoxels(IChunkColumn chunk)
	    {
		    for (int x = 0; x < 16; x++)
		    for (int z = 0; z < 16; z++)
		    for (int y = chunk.GetHeighest() + 1; y < 255; y++)
			    chunk.SetSkyLight(x, y, z, 15);
	    }

	    /// <summary>
	    /// Queues the initial lighting pass for a newly generated chunk.
	    /// </summary>
	    public void CalculateLighting(IChunkColumn chunk, bool skyLight = true, bool initial = false)
	    {
		    if (!chunk.SkyLightDirty) return;
		    // Set voxels above max height to 0xFF
			if (initial)
				SetUpperVoxels(chunk);

		    var coords = new ChunkCoordinates(chunk.X * 16, chunk.Z * 16);
		    var min = new Vector3(coords.X, 0, coords.Z);
			var max = new Vector3(coords.X + 16, chunk.GetHeighest() + 2, coords.Z + 16);

            EnqueueOperation(new ChunkCoordinates(chunk.X, chunk.Z),  new BoundingBox(min, max),
			    skyLight, initial);
	    }
    }
}
