using System;
using System.Buffers;
using System.Collections.Generic;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Networking.Java.Util;
using NLog;

namespace Alex.Worlds.Chunks
{
    public class ChunkSection
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkSection));

		protected int _blockRefCount;
		protected int _tickRefCount;

		public int Blocks => _blockRefCount;

		protected BlockStorage[] _blockStorages;
		public NibbleArray BlockLight;
		public NibbleArray SkyLight;

		private System.Collections.BitArray ScheduledUpdates;
        private System.Collections.BitArray ScheduledSkylightUpdates;
        private System.Collections.BitArray ScheduledBlocklightUpdates;

        public int ScheduledUpdatesCount { get; private set; } = 0;
        public int SkyLightUpdates { get; private set; } = 0;
        public int BlockLightUpdates { get; private set; } = 0;
        
        public List<BlockCoordinates> LightSources { get; } = new List<BlockCoordinates>();
        
		public bool IsAllAir => _blockRefCount == 0;
		private ChunkColumn Owner { get; }
        public ChunkSection(ChunkColumn owner, bool storeSkylight, int sections = 2)
        {
	        Owner = owner;
	        if (sections <= 0)
		        sections = 1;

	        //Data = new BlockStorage();
	        _blockStorages = new BlockStorage[sections];
	        for (int i = 0; i < sections; i++)
	        {
		        _blockStorages[i] = new BlockStorage();
	        }
	        
	        this.BlockLight = new NibbleArray(new byte[2048]);
	        MiNET.Worlds.ChunkColumn.Fill<byte>(BlockLight.Data, 0);
	        
		//	if (storeSkylight)
			{
				this.SkyLight = new NibbleArray(new byte[2048]);	
				MiNET.Worlds.ChunkColumn.Fill<byte>(SkyLight.Data, 0x00);
			}

		    ScheduledUpdates = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
		    ScheduledSkylightUpdates = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
		    ScheduledBlocklightUpdates = new System.Collections.BitArray(new byte[(16 * 16 * 16) / 8 ]);
        }

        protected static int GetCoordinateIndex(int x, int y, int z)
		{
			return (y << 8 | z << 4 | x);
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
				BlockLightUpdates--;
			}
			else if (!oldValue && value)
			{
				BlockLightUpdates++;
			}
			
			ScheduledBlocklightUpdates.Set(idx, value);
		}

		public bool IsSkylightUpdateScheduled(int x, int y, int z)
		{
		    return
		        ScheduledSkylightUpdates.Get(GetCoordinateIndex(x, y,
		            z));
		}

		public bool SetSkyLightUpdateScheduled(int x, int y, int z, bool value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldValue = ScheduledSkylightUpdates[idx];

			if (oldValue && !value)
			{
				SkyLightUpdates--;
			}
			else if (!oldValue && value)
			{
				SkyLightUpdates++;
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
				state = BlockFactory.GetBlockState("minecraft:air");
			}

			//var coordsIndex = GetCoordinateIndex(x, y, z);

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
					}
				}

				OnBlockSet(x, y, z, state, iblockstate);
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
	            }
            }

            if (state != null)
            {
	            _blockStorages[storage].Set(x, y, z, state);
            }

            //ScheduledUpdates.Set(coordsIndex, true);
            SetScheduled(x,y,z, true);
		}

		protected virtual void OnBlockSet(int x, int y, int z, BlockState newState, BlockState oldState)
		{
			
		}

		public bool IsEmpty()
		{
			return this._blockRefCount == 0;
		}
        
		public bool NeedsRandomTick()
		{
			return this._tickRefCount > 0;
		}

		public bool SetSkylight(int x, int y, int z, int value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldSkylight = this.SkyLight[idx];
			if (value != oldSkylight)
			{
				this.SkyLight[idx] = (byte) value;
				SetSkyLightUpdateScheduled(x, y, z, true);

				return true;
			}

			return false;
		}

		public byte GetSkylight(int x, int y, int z)
		{
			return this.SkyLight[GetCoordinateIndex(x,y,z)];
		}
		
		public bool SetBlocklight(int x, int y, int z, byte value)
		{
			var idx = GetCoordinateIndex(x, y, z);
			
			var oldBlocklight = this.BlockLight[idx];
			if (oldBlocklight != value)
			{
				this.BlockLight[idx] = value;
				SetBlockLightScheduled(x, y, z, true);

				return true;
			}

			return false;
		}
		
		public int GetBlocklight(int x, int y, int z)
		{
			return this.BlockLight[GetCoordinateIndex(x,y,z)];
		}

		public virtual void RemoveInvalidBlocks()
		{
			this._blockRefCount = 0;
			this._tickRefCount = 0;

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					for (int z = 0; z < 16; z++)
					{
						var block = this.Get(x, y, z, 0).Block;

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
		}

		public void Dispose()
	    {
		    for (int i = 0; i < _blockStorages.Length; i++)
		    {
			    _blockStorages[i]?.Dispose();
		    }
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
