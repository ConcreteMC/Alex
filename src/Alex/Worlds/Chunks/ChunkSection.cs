using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Networking.Java.Util;
using Alex.Worlds.Lighting;
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
		protected readonly BiomeStorage[] BiomeStorages;
		
		public   readonly LightArray    BlockLight;
		public   readonly LightArray    SkyLight;

		public List<BlockCoordinates> LightSources { get; private set; } = new List<BlockCoordinates>();
        
		public bool IsAllAir => BlockRefCount == 0;
		
        public ChunkSection(int sections = 1)
        {
	        if (sections <= 0)
		        sections = 1;

	        BlockStorages = new BlockStorage[sections];
	        BiomeStorages = new BiomeStorage[sections];
	        for (int i = 0; i < sections; i++)
	        {
		        BlockStorages[i] = new BlockStorage();
		        BiomeStorages[i] = new BiomeStorage();
	        }
	        
	        this.BlockLight = new LightArray();
	        this.SkyLight = new LightArray();
	        
	        //BiomeIds = new int[16 * 16 * 16];

	        ResetLight(true, true);
        }

        internal void ResetLight(bool blockLight, bool skyLight)
        {
	        if (blockLight)
				BlockLight.Reset(0);
	        
	        if (skyLight)
		        SkyLight.Reset(0);
        }

        internal static int GetCoordinateIndex(int x, int y, int z)
		{
			return (y << 8 | z << 4 | x);
		}

        public Biome GetBiome(int x, int y, int z)
        {
	        return BiomeStorages[0].Get(x, y, z);
	       // return BiomeIds[GetCoordinateIndex(x, y, z)];
        }

        public void SetBiome(int x, int y, int z, Biome biome)
        {
	        BiomeStorages[0].Set(x, y, z, biome);
	       // BiomeIds[GetCoordinateIndex(x, y, z)] = biomeId;
        }
        
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

	        return BlockStorages[storage]?.Get(x, y, z);
        }

        public void Set(int x, int y, int z, BlockState state)
        {
	        Set(0, x, y, z, state);
        }

		public void Set(int storage, int x, int y, int z, BlockState state)
		{
			if (storage > BlockStorages.Length)
				throw new IndexOutOfRangeException($"The storage id {storage} does not exist!");
			
			var blockStorage = BlockStorages[storage];
			if (blockStorage == null)
				return;

			var blockCoordinates = new BlockCoordinates(x, y, z);
			
			if (state == null)
			{
				state = BlockFactory.GetBlockState("minecraft:air");
			}

			if (storage == 0)
			{
				var lightSources = LightSources;
				if (state.Block.Luminance > 0)
				{
					if (lightSources != null && !lightSources.Contains(blockCoordinates))
					{
						lightSources.Add(blockCoordinates);
					}

					if (state.Block.Luminance > GetBlocklight(x, y, z))
					{
						SetBlocklight(x, y, z, (byte) state.Block.Luminance);
					}
				}
				else
				{
					if (lightSources != null && lightSources.Contains(blockCoordinates))
					{
						lightSources.Remove(blockCoordinates);
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

            blockStorage.Set(x, y, z, state);
		}

		protected virtual void OnBlockSet(int x, int y, int z, BlockState newState, BlockState oldState)
		{
			
		}
		
		public bool SetSkylight(int x, int y, int z, int value)
		{
			var idx = GetCoordinateIndex(x, y, z);

			var oldSkylight = this.SkyLight[idx];
			if (value != oldSkylight)
			{
				this.SkyLight[idx] = (byte) value;
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
			var lightSources = LightSources;

			if (lightSources == null)
				return;
			
			this.BlockRefCount = 0;

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					for (int z = 0; z < 16; z++)
					{
						var blockState = this.Get(x, y, z, 0);//.Block;
						if (blockState?.Block == null)
							continue;

						var block = blockState.Block;

						if (!(block is Air))
						{
							++this.BlockRefCount;
						}
						
						
						if (block.Luminance > 0)
						{
							var coords = new BlockCoordinates(x, y, z);

							if (!lightSources.Contains(coords))
							{
								lightSources.Add(coords);
							}

							if (GetBlocklight(x, y, z) < block.Luminance)
							{
								SetBlocklight(x, y, z, (byte) block.Luminance);
							}
						}
					}
				}
			}
		}

		private bool _disposed = false;

		public void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (!disposing)
			{
				Log.Warn($"Dispose was never called. Cleaning up.");
			}

			for (int i = 0; i < BlockStorages.Length; i++)
			{
				BlockStorages[i]?.Dispose();
				BlockStorages[i] = null;
			}

			LightSources?.Clear();
			LightSources = null;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		~ChunkSection()
		{
			Dispose(false);
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
