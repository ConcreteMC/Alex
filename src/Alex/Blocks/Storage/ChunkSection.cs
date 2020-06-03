using System;
using System.Buffers;
using System.Collections.Generic;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Networking.Java.Util;
using Alex.Worlds;
using NLog;

namespace Alex.Blocks.Storage
{
    public class ChunkSection
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkSection));

		private int _yBase;
		private int _blockRefCount;
		private int _tickRefCount;

		public int Blocks => _blockRefCount;
		
		//private BlockStorage Data;
		private BlockStorage[] _blockStorages;
		public NibbleArray BlockLight;
		public NibbleArray SkyLight;

        public System.Collections.BitArray TransparentBlocks;
        public System.Collections.BitArray SolidBlocks;
        public System.Collections.BitArray RenderedBlocks;
        
        private System.Collections.BitArray ScheduledUpdates;
        private System.Collections.BitArray ScheduledSkylightUpdates;
        private System.Collections.BitArray ScheduledBlocklightUpdates;

        public int ScheduledUpdatesCount { get; private set; } = 0;
        public int ScheduledSkyUpdatesCount { get; private set; } = 0;
        public int ScheduledBlockUpdatesCount { get; private set; } = 0;
        
        public List<BlockCoordinates> LightSources { get; } = new List<BlockCoordinates>();
        
		public bool IsAllAir => _blockRefCount == 0;

		private ChunkMesh _meshCache = null;

		internal ChunkMesh MeshCache
		{
			get
			{
				return _meshCache;
			}
			set
			{
				var oldValue = _meshCache;
				_meshCache = value;

				if (!ReferenceEquals(oldValue, value))
				{
					oldValue?.Dispose();
				}
			}
		}
		//internal Dictionary<BlockCoordinates, IList<ChunkMesh.EntryPosition>> MeshPositions { get; set; } = null;
		
		private ChunkColumn Owner { get; }
        public ChunkSection(ChunkColumn owner, int y, bool storeSkylight, int sections = 2)
        {
	        Owner = owner;
	        if (sections <= 0)
		        sections = 1;
	        
	        this._yBase = y;
	        //Data = new BlockStorage();
	        _blockStorages = new BlockStorage[sections];
	        for (int i = 0; i < sections; i++)
	        {
		        _blockStorages[i] = new BlockStorage();
	        }
	        
	        this.BlockLight = new NibbleArray(ArrayPool<byte>.Shared.Rent(2048));
	        MiNET.Worlds.ChunkColumn.Fill<byte>(BlockLight.Data, 0);
	        
			if (storeSkylight)
			{
				this.SkyLight = new NibbleArray(ArrayPool<byte>.Shared.Rent(2048));	
				MiNET.Worlds.ChunkColumn.Fill<byte>(SkyLight.Data, 0xff);
			}
//System.Collections.BitArray a = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8]);

		    TransparentBlocks = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
		    SolidBlocks = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
		    ScheduledUpdates = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
		    ScheduledSkylightUpdates = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
		    ScheduledBlocklightUpdates = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
            RenderedBlocks = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
		
            for (int i = 0; i < TransparentBlocks.Length; i++)
			{
				TransparentBlocks[i] = true;
				SolidBlocks[i] = false;
			}
        }

        private bool _isDirty = false;
        public bool IsDirty
        {
	        get
	        {
		        return New || ScheduledUpdatesCount > 0 || ScheduledBlockUpdatesCount > 0 ||
		               ScheduledSkyUpdatesCount > 0 || _isDirty;
	        }
	        set
	        {
		        _isDirty = value;
	        }
        }

        public bool New { get; set; } = true;

        public void ResetSkyLight(byte initialValue = 0xff)
        {
	        Owner.SkyLightDirty = true;
	        MiNET.Worlds.ChunkColumn.Fill<byte>(SkyLight.Data, initialValue);
			//this.SkyLight = new NibbleArray(4096, initialValue);
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
			var idx = GetCoordinateIndex(x, y, z);

			var oldValue = ScheduledUpdates[idx];

			if (oldValue && !value)
			{
				ScheduledUpdatesCount--;
			}
			else if (!oldValue && value)
			{
				ScheduledUpdatesCount++;
			}
			
            ScheduledUpdates.Set(idx, value);
		}

		public bool IsBlockLightScheduled(int x, int y, int z)
		{
			return ScheduledBlocklightUpdates.Get(GetCoordinateIndex(x, y, z));
		}

		public void SetBlockLightScheduled(int x, int y, int z, bool value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldValue = ScheduledBlocklightUpdates[idx];

			if (oldValue && !value)
			{
				ScheduledBlockUpdatesCount--;
			}
			else if (!oldValue && value)
			{
				ScheduledBlockUpdatesCount++;
			}
			
			ScheduledBlocklightUpdates.Set(idx, value);
		}

		public bool IsLightingScheduled(int x, int y, int z)
		{
		    return
		        ScheduledSkylightUpdates.Get(GetCoordinateIndex(x, y,
		            z));
		}

		public bool SetLightingScheduled(int x, int y, int z, bool value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldValue = ScheduledSkylightUpdates[idx];

			if (oldValue && !value)
			{
				ScheduledSkyUpdatesCount--;
			}
			else if (!oldValue && value)
			{
				ScheduledSkyUpdatesCount++;
			}
			
			ScheduledSkylightUpdates.Set(idx, value);

			return value;
		}

        public BlockState Get(int x, int y, int z)
		{
			return this.Get(x, y, z, 0);
		}

        public IEnumerable<BlockEntry> GetAll(int x, int y, int z)
        {
	        for (int i = 0; i < _blockStorages.Length; i++)
	        {
		        yield return new BlockEntry(Get(x, y, z, i), i);
	        }
        }

        public BlockState Get(int x, int y, int z, int section)
        {
	        if (section > _blockStorages.Length)
		        throw new IndexOutOfRangeException($"The storage id {section} does not exist!");

	        return _blockStorages[section].Get(x, y, z);
        }

        public void Set(int x, int y, int z, BlockState state)
        {
	        Set(0, x, y, z, state);
        }

		public void Set(int storage, int x, int y, int z, BlockState state)
		{
			if (storage > _blockStorages.Length)
				throw new IndexOutOfRangeException($"The storage id {storage} does not exist!");
			
			var blockCoordinates = new BlockCoordinates(x, y, z);
			
			if (state == null)
			{
				Log.Warn($"State == null");
				return;
			}

			var coordsIndex = GetCoordinateIndex(x, y, z);

			if (storage == 0)
			{
				if (state.Block.LightValue > 0)
				{
					if (!LightSources.Contains(blockCoordinates))
					{
						LightSources.Add(blockCoordinates);
					}

					SetBlocklight(x,y,z, (byte) state.Block.LightValue);
					SetBlockLightScheduled(x,y,z, true);
				}
				else
				{
					if (LightSources.Contains(blockCoordinates))
						LightSources.Remove(blockCoordinates);
				}
				
				BlockState iblockstate = this.Get(x, y, z, storage);
				if (iblockstate != null)
				{
					Block block = iblockstate.Block;

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
			}

			Block block1 = state.Block;
            if (storage == 0)
            {
	            if (!(block1 is Air))
	            {
		            ++this._blockRefCount;

		            if (block1.RandomTicked)
		            {
			            ++this._tickRefCount;
		            }

		            TransparentBlocks.Set(coordsIndex, block1.Transparent);
		            SolidBlocks.Set(coordsIndex, block1.Solid);
	            }
            }

            _blockStorages[storage].Set(x, y, z, state);

            //ScheduledUpdates.Set(coordsIndex, true);
            SetScheduled(x,y,z, true);
            
            IsDirty = true;
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

		public bool SetSkylight(int x, int y, int z, int value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldSkylight = this.SkyLight[idx];
			if (value != oldSkylight)
			{
				this.SkyLight[idx] = (byte) value;
				SetLightingScheduled(x, y, z, true);
				//ScheduledSkylightUpdates.Set(idx, true);

				//Owner.SkyLightDirty = true;

				return true;
			}

			return false;
		}

		public byte GetSkylight(int x, int y, int z)
		{
			return this.SkyLight[GetCoordinateIndex(x,y,z)]; //.get(x, y, z);
		}
		
		public bool SetBlocklight(int x, int y, int z, byte value)
		{
			var idx = GetCoordinateIndex(x, y, z);
			
			var oldBlocklight = this.BlockLight[idx];
			if (oldBlocklight != value)
			{
				this.BlockLight[idx] = value;
				SetBlockLightScheduled(x, y, z, true);
				//ScheduledBlocklightUpdates.Set(idx, true);

				Owner.BlockLightDirty = true;

				return true;
			}

			return false;
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
						var idx = GetCoordinateIndex(x, y, z);
						
						//foreach (var state in this.GetAll(x, y, z))
						{
							var block = this.Get(x,y,z, 0).Block;

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

							if (block.LightValue > 0)
							{
								var coords = new BlockCoordinates(x,y,z);

								if (!LightSources.Contains(coords))
								{
									LightSources.Add(coords);
								}

								if (GetBlocklight(x, y, z) != block.LightValue)
								{
									SetBlocklight(x,y,z, (byte) block.LightValue);
									SetBlockLightScheduled(x,y,z, true);
								}
							}
						}
					}
				}
			}
			
			//CheckForSolidBorder();
		}

		public void Read(MinecraftStream ms)
	    {
		    _blockStorages[0].Read(ms);
	    }

	    public void Dispose()
	    {
		    for (int i = 0; i < _blockStorages.Length; i++)
		    {
			    _blockStorages[i]?.Dispose();
		    }
		    
		    MeshCache?.Dispose();
		    
		    ArrayPool<byte>.Shared.Return(BlockLight.Data);
		    ArrayPool<byte>.Shared.Return(SkyLight.Data);
	    }

	    public class BlockEntry
	    {
		    public BlockState State { get; set; }
		    public int Storage { get; set; }

		    public BlockEntry(BlockState state, int storage)
		    {
			    State = state;
			    Storage = storage;
		    }
	    }
    }
}
