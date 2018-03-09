using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Blocks.Storage.Pallete;
using log4net;
using MiNET.Utils;

namespace Alex.Blocks.Storage
{
	public class BlockStateContainer : IBlockStatePaletteResizer
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BlockStateContainer));
		private static readonly IBlockStatePalette RegistryBasedPalette = new BlockStatePaletteRegistry();
		protected static IBlockState AirBlockState = new Air().GetDefaultState();
		protected FlexibleStorage Storage;
		protected IBlockStatePalette Palette;
		private int _bits;

		public BlockStateContainer()
		{
			this.SetBits(8);
		}

		private static int GetIndex(int x, int y, int z)
		{
			return y << 8 | z << 4 | x;
		}

		private void SetBits(int bitsIn)
		{
			if (bitsIn != this._bits)
			{
				this._bits = bitsIn;

				if (this._bits <= 4)
				{
					this._bits = 4;
					this.Palette = new BlockStatePaletteLinear(this._bits, this);
				}
				else if (this._bits <= 8)
				{
					this.Palette = new BlockStatePaletteHashMap(this._bits, this);
				}
				else
				{
					this.Palette = RegistryBasedPalette;
					this._bits = 13; //MathHelper.Log2E(Block.BLOCK_STATE_IDS.size());
				}

				this.Palette.IdFor(AirBlockState);
				this.Storage = new FlexibleStorage(_bits, 4096);
			}
		}

		public int OnResize(int bits, IBlockState state)
		{
			FlexibleStorage bitarray = this.Storage;
			IBlockStatePalette blockStatepalette = this.Palette;
			this.SetBits(bits);

			for (int i = 0; i < bitarray.Size(); i++)
			{
				IBlockState blockState = blockStatepalette.GetBlockState(bitarray[i]);

				if (blockState != null)
				{
					this.Set(i, blockState);
				}
			}

			return this.Palette.IdFor(state);
		}

		public void Set(int x, int y, int z, IBlockState state)
		{
			this.Set(GetIndex(x, y, z), state);
		}

		protected void Set(int index, IBlockState state)
		{
			if (state == null)
			{
				return;
			}

			int i = this.Palette.IdFor(state);
			this.Storage[index] = i;
		}

		public IBlockState Get(int x, int y, int z)
		{
			return this.Get(GetIndex(x, y, z));
		}

		protected IBlockState Get(int index)
		{
			IBlockState blockState = this.Palette.GetBlockState(this.Storage[index]);
			return blockState ?? AirBlockState;
		}

		/*public void read(PacketBuffer buf)
		{
			int i = buf.readByte();
	
			if (this.bits != i)
			{
				this.setBits(i);
			}
	
			this.palette.read(buf);
			buf.readLongArray(this.storage.getBackingLongArray());
		}
	
		public void write(PacketBuffer buf)
		{
			buf.writeByte(this.bits);
			this.palette.write(buf);
			buf.writeLongArray(this.storage.getBackingLongArray());
		}*/

		public NibbleArray GetDataForNbt(byte[] blockIds, NibbleArray data)
		{
			NibbleArray nibblearray = null;

			for (int i = 0; i < 4096; i++)
			{
				//BlockFactory.
				int blockStateId = BlockFactory.GetBlockStateId(this.Get(i));
				int x = i & 15;
				int y = i >> 8 & 15;
				int z = i >> 4 & 15;

				if ((blockStateId >> 12 & 15) != 0)
				{
					if (nibblearray == null)
					{
						nibblearray = new NibbleArray();
					}

					nibblearray[GetIndex(x, y, z)] = (byte) ((blockStateId >> 12) & 15);
				}

				blockIds[i] = (byte) ((blockStateId >> 4) & 255);
				data[GetIndex(x, y, z)] = (byte) (blockStateId & 15); // .Set(k, l, i1, j & 15);
			}

			return nibblearray;
		}

		public void SetDataFromNbt(byte[] blocks, byte[] data, byte[] blockIdExtension)
		{
			for (int i = 0; i < 4096; i++)
			{
				int blockIdExtensionData = blockIdExtension == null ? 0 : Nibble4(blockIdExtension, i); //.Get(j, k, l);
				int blockStateId = (blockIdExtensionData << 12) | ((blocks[i]) << 4) | Nibble4(data, i); //.Get(j, k, l);

				this.Set(i, BlockFactory.GetBlockState(blockStateId));
			}
		}

		private static byte Nibble4(byte[] arr, int index)
		{
			return (byte)(arr[index >> 1] >> ((index & 1) << 2) & 0xF);
		}

		public int GetSerializedSize()
		{
			return 1 + this.Palette.GetSerializedSize() + BlockState.GetVarIntSize(this.Storage.Size()) +
			       this.Storage.GetBackingLongArray().Length * 8;
		}

		public IBlockState GetBlockState(int indexKey)
		{
			IBlockState iblockstate = BlockFactory.GetBlock((uint) indexKey).BlockState; //.getByValue(indexKey);
			return iblockstate == null ? new Air().GetDefaultState() : iblockstate;
		}
	}
}
