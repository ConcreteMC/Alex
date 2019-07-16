using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Microsoft.Xna.Framework;
using NLog;
using BitArray = Alex.API.Utils.BitArray;

namespace Alex.Blocks.Storage
{
    public class ChunkSection : IChunkSection
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkSection));

		private int _yBase;
		private int _blockRefCount;
		private int _tickRefCount;
		
		private BlockStorage Data;
		public NibbleArray BlockLight;
		public NibbleArray SkyLight;

        public BitArray TransparentBlocks;
        public BitArray SolidBlocks;
        public BitArray ScheduledUpdates;
        public BitArray ScheduledSkylightUpdates;
        public BitArray RenderedBlocks;

        public bool SolidBorder { get; private set; } = false;
		private bool[] FaceSolidity { get; set; } = new bool[6];
		public bool HasAirPockets { get; private set; } = true;

		internal ChunkMesh MeshCache { get; set; } = null;
		internal IReadOnlyDictionary<Vector3, ChunkMesh.EntryPosition> MeshPositions { get; set; } = null;
		
        public ChunkSection(int y, bool storeSkylight)
        {
	        this._yBase = y;
	        Data = new BlockStorage();
	        this.BlockLight = new NibbleArray(4096, 0);

			if (storeSkylight)
			{
				this.SkyLight = new NibbleArray(4096, 0xff);
			}

		    TransparentBlocks = new BitArray(new byte[(16 * 16 * 16) / 8]);
		    SolidBlocks = new BitArray(new byte[(16 * 16 * 16) / 8]);
		    ScheduledUpdates = new BitArray(new byte[(16 * 16 * 16) / 8]);
		    ScheduledSkylightUpdates = new BitArray(new byte[(16 * 16 * 16) / 8]);
            RenderedBlocks = new BitArray(new byte[(16 * 16 * 16) / 8]);

            for (int i = 0; i < TransparentBlocks.Length; i++)
			{
				TransparentBlocks[i] = true;
				SolidBlocks[i] = false;
			}
        }

        public bool IsDirty { get; set; }
        public bool New { get; set; } = true;

        public void ResetSkyLight()
		{
			this.SkyLight = new NibbleArray(4096, 0);
		}

		private static int GetCoordinateIndex(int x, int y, int z)
		{
			return (y << 8 | z << 4 | x);
		}

        public void SetRendered(int x, int y, int z, bool value)
        {
            RenderedBlocks[GetCoordinateIndex(x, y, z)] = value;
        }

        public bool IsRendered(int x, int y, int z)
        {
            return RenderedBlocks[GetCoordinateIndex(x, y, z)];
        }

        public bool IsScheduled(int x, int y, int z)
		{
		    return ScheduledUpdates.Get(GetCoordinateIndex(x, y, z));
		}

		public void SetScheduled(int x, int y, int z, bool value)
		{
            ScheduledUpdates.Set(GetCoordinateIndex(x,y,z), value);
		}

		public bool IsLightingScheduled(int x, int y, int z)
		{
		    return
		        ScheduledSkylightUpdates.Get(GetCoordinateIndex(x, y,
		            z));
		}

		public bool SetLightingScheduled(int x, int y, int z, bool value)
		{
		    ScheduledSkylightUpdates.Set(GetCoordinateIndex(x, y, z),
		        value);

		    return value;
		}

        public IBlockState Get(int x, int y, int z)
		{
			return this.Data.Get(x, y, z);
		}

		public void Set(int x, int y, int z, IBlockState state)
		{
			if (state == null)
			{
				Log.Warn($"State == null");
				return;
			}

			var coordsIndex = GetCoordinateIndex(x, y, z);

            IBlockState iblockstate = this.Get(x, y, z);
			if (iblockstate != null)
			{
				IBlock block = iblockstate.Block;

				if (!(block is Air))
				{
					--this._blockRefCount;

					if (block.RandomTicked)
					{
						--this._tickRefCount;
					}
					
				    TransparentBlocks.Set(coordsIndex, true);
				    SolidBlocks.Set(coordsIndex, false);
                }				
			}

			IBlock block1 = state.Block;
			if (!(block1 is Air))
			{
				++this._blockRefCount;

				if (block1.RandomTicked)
				{
					++this._tickRefCount;
				}
				
			    TransparentBlocks.Set(coordsIndex, block1.Transparent);
			    SolidBlocks.Set(coordsIndex, block1.Transparent);
			}
			
			this.Data.Set(x, y, z, state);

            ScheduledUpdates.Set(coordsIndex, true);
            IsDirty = true;
			
			if (!block1.Solid)
			{
				HasAirPockets = true;
			}
		}

		public bool IsTransparent(int x, int y, int z)
		{
			return TransparentBlocks.Get(GetCoordinateIndex(x, y, z));
		}

		public bool IsSolid(int x, int y, int z)
		{
		    return SolidBlocks.Get(GetCoordinateIndex(x, y, z));
		}

		public void GetBlockData(int bx, int by, int bz, out bool transparent, out bool solid)
		{
			var coords = GetCoordinateIndex(bx, by, bz);
		    transparent = TransparentBlocks.Get(coords);// TransparentBlocks[coords];
		    solid = SolidBlocks.Get(coords);// SolidBlocks[coords];
		}
		
        public bool IsEmpty()
		{
			return this._blockRefCount == 0;
		}
        
		public bool NeedsRandomTick()
		{
			return this._tickRefCount > 0;
		}

		public int GetYLocation()
		{
			return this._yBase;
		}

		public void SetSkylight(int x, int y, int z, int value)
		{
			var idx = GetCoordinateIndex(x, y, z);

            this.SkyLight[idx] = (byte) value;
            ScheduledSkylightUpdates.Set(idx, true);
		}

		public byte GetSkylight(int x, int y, int z)
		{
			return this.SkyLight[GetCoordinateIndex(x,y,z)]; //.get(x, y, z);
		}
		
		public void SetBlocklight(int x, int y, int z, byte value)
		{
			this.BlockLight[GetCoordinateIndex(x,y,z)] = value;
		}
		
		public int GetBlocklight(int x, int y, int z)
		{
			return this.BlockLight[GetCoordinateIndex(x,y,z)];
		}

		public void RemoveInvalidBlocks()
		{
			this._blockRefCount = 0;
			this._tickRefCount = 0;

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					for (int z = 0; z < 16; z++)
					{
						IBlock block = this.Get(x, y, z).Block;
						
						var idx = GetCoordinateIndex(x, y, z);
						
                        TransparentBlocks.Set(idx, block.Transparent);
                        SolidBlocks.Set(idx, block.Solid);

                        if (!(block is Air))
						{
							++this._blockRefCount;

							if (block.RandomTicked)
							{
								++this._tickRefCount;
							}
						}
					}
				}
			}
			
			CheckForSolidBorder();
		}
		
		private void CheckForSolidBorder()
	    {
	        bool[] solidity = new bool[6]
	        {
	            true,
	            true,
	            true,
	            true,
	            true,
	            true
	        };

	        for (int y = 0; y < 16; y++)
	        {
	            for (int x = 0; x < 16; x++)
	            {
	                if (!SolidBlocks[GetCoordinateIndex(x, y, 0)])
	                {
	                    solidity[2] = false;
	                    SolidBorder = false;
                    }

	                if (!SolidBlocks[GetCoordinateIndex(0, y, x)])
	                {
	                    SolidBorder = false;
	                    solidity[4] = false;
                    }

	                if (!SolidBlocks[GetCoordinateIndex(x, y, 15)])
	                {
	                    SolidBorder = false;
	                    solidity[3] = false;
                    }

	                if (!SolidBlocks[GetCoordinateIndex(15, y, x)])
	                {
	                    SolidBorder = false;
	                    solidity[5] = false;
                    }

	                for (int xx = 0; xx < 16; xx++)
	                {
	                    if (!SolidBlocks[GetCoordinateIndex(xx, 0, x)])
	                    {
	                        SolidBorder = false;
	                        solidity[0] = false;
	                    }

                        if (!SolidBlocks[GetCoordinateIndex(xx, 15, x)])
	                    {
	                        SolidBorder = false;
	                        solidity[1] = false;
	                    }
	                }
	            }
	        }

	        bool airPockets = false;

	        for (int x = 1; x < 15; x++)
	        {
	            for (int y = 1; y < 15; y++)
	            {
	                for (int z = 1; z < 15; z++)
	                {
	                    if (!SolidBlocks[GetCoordinateIndex(x, y, z)])
	                    {
	                        airPockets = true;
	                        break;
	                    }
	                }
                    if (airPockets)
                        break;
	            }

	            if (airPockets)
	                break;
	        }

	        FaceSolidity = solidity;
	        HasAirPockets = airPockets;
	    }

	    public bool IsFaceSolid(BlockFace face)
	    {
	        var intFace = (int) face;

            if (face == BlockFace.None || intFace < 0 || intFace > 5) return false;
	        return FaceSolidity[(int)intFace];
	    }
    }
}
