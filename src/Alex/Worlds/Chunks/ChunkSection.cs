using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.API.Utils.Vectors;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Networking.Java.Util;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Worlds.Chunks
{
    public class ChunkSection
    {
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkSection));

		protected int BlockRefCount;

		public int Blocks => BlockRefCount;
		public int StorageCount => BlockStorages.Length;
		
		protected readonly BlockStorage[] BlockStorages;
		public             NibbleArray    BlockLight;
		public             NibbleArray    SkyLight;

		public List<BlockCoordinates> LightSources { get; private set; } = new List<BlockCoordinates>();
        
		public bool IsAllAir => BlockRefCount == 0;
		
        public ChunkSection(bool storeSkylight, int sections = 1)
        {
	        //_octree.
	        if (sections <= 0)
		        sections = 1;

	        //Data = new BlockStorage();
	        BlockStorages = new BlockStorage[sections];
	        for (int i = 0; i < sections; i++)
	        {
		        BlockStorages[i] = new BlockStorage();
	        }
	        
	        this.BlockLight = new NibbleArray(new byte[2048]);

	        //	if (storeSkylight)
			{
				this.SkyLight = new NibbleArray(new byte[2048]);
			}

			ResetLight(true, true);
        }

        internal void ResetLight(bool blockLight, bool skyLight)
        {
	        if (blockLight)
				MiNET.Worlds.ChunkColumn.Fill<byte>(BlockLight.Data, 0);
	        
	        if (skyLight)
		        MiNET.Worlds.ChunkColumn.Fill<byte>(SkyLight.Data, 0x00);
        }

        protected static int GetCoordinateIndex(int x, int y, int z)
		{
			return (y << 8 | z << 4 | x);
		}

       // public bool IsScheduled(int x, int y, int z)
		//{
		//    return _scheduledUpdates.Get(GetCoordinateIndex(x, y, z));
		//}

		/*public void SetScheduled(int x, int y, int z, bool value)
		{
			var scheduled = _scheduledUpdates;
			if (scheduled == null) return;
			
			var idx = GetCoordinateIndex(x, y, z);

			var oldValue = scheduled[idx];

			if (oldValue && !value)
			{
				Interlocked.Decrement(ref _pendingUpdates);
				//ScheduledUpdatesCount--;
			}
			else if (!oldValue && value)
			{
				Interlocked.Increment(ref _pendingUpdates);
				//ScheduledUpdatesCount++;
			}
			
			scheduled.Set(idx, value);
		}

		public bool IsBlockLightScheduled(int x, int y, int z)
		{
			return _scheduledBlocklightUpdates.Get(GetCoordinateIndex(x, y, z));
		}

		public void SetBlockLightScheduled(int x, int y, int z, bool value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldValue = _scheduledBlocklightUpdates[idx];

			if (oldValue && !value)
			{
				BlockLightUpdates--;
			}
			else if (!oldValue && value)
			{
				BlockLightUpdates++;
			}
			
			_scheduledBlocklightUpdates.Set(idx, value);
		}

		public bool IsSkylightUpdateScheduled(int x, int y, int z)
		{
		    return
		        _scheduledSkylightUpdates.Get(GetCoordinateIndex(x, y,
		            z));
		}

		public bool SetSkyLightUpdateScheduled(int x, int y, int z, bool value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldValue = _scheduledSkylightUpdates[idx];

			if (oldValue && !value)
			{
				SkyLightUpdates--;
			}
			else if (!oldValue && value)
			{
				SkyLightUpdates++;
			}
			
			_scheduledSkylightUpdates.Set(idx, value);

			return value;
		}*/

        public BlockState Get(int x, int y, int z)
		{
			return this.Get(x, y, z, 0);
		}

        public IEnumerable<BlockEntry> GetAll(int x, int y, int z)
        {
	        for (int i = 0; i < BlockStorages.Length; i++)
	        {
		        yield return new BlockEntry(Get(x, y, z, i), i);
	        }
        }

        public BlockState Get(int x, int y, int z, int storage)
        {
	        if (storage > BlockStorages.Length)
		        throw new IndexOutOfRangeException($"The storage id {storage} does not exist!");

	        return BlockStorages[storage]?.Get(x, y, z) ?? BlockFactory.GetBlockState("minecraft:air");
        }

        public void Set(int x, int y, int z, BlockState state)
        {
	        Set(0, x, y, z, state);
        }

		public void Set(int storage, int x, int y, int z, BlockState state)
		{
			if (storage > BlockStorages.Length)
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

					if (state.Block.LightValue > GetBlocklight(x, y, z))
					{
						SetBlocklight(x, y, z, (byte) state.Block.LightValue);
					}

					//SetBlockLightScheduled(x,y,z, true);
				}
				else
				{
					if (LightSources.Contains(blockCoordinates))
					{
						LightSources.Remove(blockCoordinates);
					}

					if (GetBlocklight(x, y, z) > 0)
					{
						SetBlocklight(x, y, z, 0);
					}
				}
				
				BlockState iblockstate = this.Get(x, y, z, storage);
				if (iblockstate != null)
				{
					Block block = iblockstate.Block;

					if (!(block is Air))
					{
						--this.BlockRefCount;
					}
				}

				OnBlockSet(x, y, z, state, iblockstate);
			}

			Block block1 = state.Block;
            if (storage == 0)
            {
	            if (!(block1 is Air))
	            {
		            ++this.BlockRefCount;
	            }
            }

            if (state != null)
            {
	            BlockStorages[storage].Set(x, y, z, state);
            }

            //ScheduledUpdates.Set(coordsIndex, true);
           // SetScheduled(x,y,z, true);
		}

		protected virtual void OnBlockSet(int x, int y, int z, BlockState newState, BlockState oldState)
		{

		}

		public bool IsEmpty()
		{
			return this.BlockRefCount == 0;
		}

		public bool SetSkylight(int x, int y, int z, int value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldSkylight = this.SkyLight[idx];
			if (value != oldSkylight)
			{
				this.SkyLight[idx] = (byte) value;
			//	SetSkyLightUpdateScheduled(x, y, z, true);

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
				//SetBlockLightScheduled(x, y, z, true);

				return true;
			}

			return false;
		}
		
		public byte GetBlocklight(int x, int y, int z)
		{
			return this.BlockLight[GetCoordinateIndex(x,y,z)];
		}

		public void GetLight(int x, int y, int z, out byte skyLight, out byte blockLight)
		{
			var index = GetCoordinateIndex(x, y, z);
			blockLight = this.BlockLight[index];
			skyLight = this.SkyLight[index];
		}

		public virtual void RemoveInvalidBlocks()
		{
			this.BlockRefCount = 0;

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					for (int z = 0; z < 16; z++)
					{
						var block = this.Get(x, y, z, 0).Block;

						if (!(block is Air))
						{
							++this.BlockRefCount;
						}
						
						
						if (block.LightValue > 0)
						{
							var coords = new BlockCoordinates(x, y, z);

							if (!LightSources.Contains(coords))
							{
								LightSources.Add(coords);
							}

							if (GetBlocklight(x, y, z) < block.LightValue)
							{
								SetBlocklight(x, y, z, (byte) block.LightValue);
								//SetBlockLightScheduled(x, y, z, true);
							}
						}
					}
				}
			}
		}

		public void Dispose()
	    {
		    for (int i = 0; i < BlockStorages.Length; i++)
		    {
			    BlockStorages[i]?.Dispose();
			    BlockStorages[i] = null;
		    }
		    
		    LightSources.Clear();
		    LightSources = null;

		    BlockLight = null;
		    SkyLight = null;
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
