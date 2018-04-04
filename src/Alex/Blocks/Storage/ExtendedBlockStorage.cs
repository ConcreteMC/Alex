using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Utils;
using NLog;

namespace Alex.Blocks.Storage
{
	public class ExtendedBlockStorage
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ExtendedBlockStorage));
		/**
		 * Contains the bottom-most Y block represented by this ExtendedBlockStorage. Typically a multiple of 16.
		 */
		private int _yBase;

		/**
		 * A total count of the number of non-air blocks in this block storage's Chunk.
		 */
		private int _blockRefCount;

		/**
		 * Contains the number of blocks in this block storage's parent chunk that require random ticking. Used to cull the
		 * Chunk from random tick updates for performance reasons.
		 */
		private int _tickRefCount;
		public BlockStateContainer Data;

		/** The NibbleArray containing a block of Block-light data. */
		public NibbleArray BlockLight;

		/** The NibbleArray containing a block of Sky-light data. */
		public NibbleArray SkyLight;

		public ExtendedBlockStorage(int y, bool storeSkylight)
		{
			this._yBase = y;
			this.Data = new BlockStateContainer();
			this.BlockLight = new NibbleArray(4096);

			if (storeSkylight)
			{
				this.SkyLight = new NibbleArray(4096);
			}
		}

		private static int GetCoordinateIndex(int x, int y, int z)
		{
			return (y << 8) + (z << 4) + x;
		}

		public IBlockState Get(int x, int y, int z)
		{
			return this.Data.Get(x, y, z);
		}

		public void Set(int x, int y, int z, IBlockState state)
		{
			if (state == null)
			{
				//Log.Warn($"State == null");
				return;
			}

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
			}

			this.Data.Set(x, y, z, state);
		}

		/**
		 * Returns whether or not this block storage's Chunk is fully empty, based on its internal reference count.
		 */
		public bool IsEmpty()
		{
			return this._blockRefCount == 0;
		}

		/**
		 * Returns whether or not this block storage's Chunk will require random ticking, used to avoid looping through
		 * random block ticks when there are no blocks that would randomly tick.
		 */
		public bool NeedsRandomTick()
		{
			return this._tickRefCount > 0;
		}

		/**
		 * Returns the Y location of this ExtendedBlockStorage.
		 */
		public int GetYLocation()
		{
			return this._yBase;
		}

		/**
		 * Sets the saved Sky-light value in the extended block storage structure.
		 */
		public void SetExtSkylightValue(int x, int y, int z, int value)
		{
			this.SkyLight[GetCoordinateIndex(x,y,z)] = (byte) value;//.Set(x, y, z, value);
		}

		/**
		 * Gets the saved Sky-light value in the extended block storage structure.
		 */
		public byte GetExtSkylightValue(int x, int y, int z)
		{
			return this.SkyLight[GetCoordinateIndex(x,y,z)]; //.get(x, y, z);
		}

		/**
		 * Sets the saved Block-light value in the extended block storage structure.
		 */
		public void SetExtBlocklightValue(int x, int y, int z, byte value)
		{
			this.BlockLight[GetCoordinateIndex(x,y,z)] = value;//.set(x, y, z, value);
		}

		/**
		 * Gets the saved Block-light value in the extended block storage structure.
		 */
		public int GetExtBlocklightValue(int x, int y, int z)
		{
			return this.BlockLight[GetCoordinateIndex(x,y,z)];// .get(x, y, z);
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
	}
}
