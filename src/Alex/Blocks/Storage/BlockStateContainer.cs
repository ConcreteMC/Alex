using System;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Blocks.Storage.Pallete;
using Alex.Networking.Java.Util;
using Alex.Utils;
using fNbt.Tags;

namespace Alex.Blocks.Storage
{
	public class BlockStateContainer : IBlockStatePaletteResizer
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockStateContainer));
		private static readonly IBlockStatePalette RegistryBasedPalette = new BlockStatePaletteRegistry();
		protected static IBlockState AirBlockState = new Air().GetDefaultState();
		protected FlexibleStorage Storage;
		protected IBlockStatePalette Palette;
		private int _bits;

		public BlockStateContainer()
		{
			this.SetBits(4);
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
					this.Palette = new BlockStatePaletteMap(this._bits, this);
				}
				else
				{
					this.Palette = RegistryBasedPalette;
					this._bits = QuickMath.Log2(QuickMath.NextPow2(BlockFactory.AllBlockstates.Count));
				}

				this.Palette.IdFor(AirBlockState);
				this.Storage = new FlexibleStorage(_bits, 4096);
			}
		}

		public uint OnResize(int bits, IBlockState state)
		{
			FlexibleStorage storage = this.Storage;
			IBlockStatePalette blockStatepalette = this.Palette;
			this.SetBits(bits);

			for (int i = 0; i < storage.Length; i++)
			{
				IBlockState blockState = blockStatepalette.GetBlockState(storage[i]);

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

		public void Set(int index, IBlockState state)
		{
			if (state == null)
			{
				return;
			}

			uint i = this.Palette.IdFor(state);
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

		public NibbleArray GetDataForNbt(byte[] blockIds, NibbleArray data)
		{
			NibbleArray nibblearray = null;

			for (int i = 0; i < 4096; i++)
			{
				//BlockFactory.
				uint blockStateId = this.Get(i).ID;
				int x = i & 15;
				int y = i >> 8 & 15;
				int z = i >> 4 & 15;

				if ((blockStateId >> 12 & 15) != 0)
				{
					if (nibblearray == null)
					{
						nibblearray = new NibbleArray();
					}

					nibblearray[GetIndex(x, y, z)] = (byte)((blockStateId >> 12) & 15);
				}

				blockIds[i] = (byte)((blockStateId >> 4) & 255);
				data[GetIndex(x, y, z)] = (byte)(blockStateId & 15); // .Set(k, l, i1, j & 15);
			}

			return nibblearray;
		}

		public void SetDataFromNbt(NbtList palette, long[] blockStates)
		{
			int bits = 4;

			if (palette.Count > 16)
			{ 
				bits = QuickMath.Log2(QuickMath.NextPow2(palette.Count));
			}

			Storage = new FlexibleStorage(bits, blockStates);
			if (bits <= 4)
			{
				this.Palette = new BlockStatePaletteLinear(bits, this);
			}
			else if (bits <= 8)
			{
				this.Palette = new BlockStatePaletteMap(bits, this);
			}
			else
			{
				this.Palette = RegistryBasedPalette;
				this._bits = QuickMath.Log2(QuickMath.NextPow2(BlockFactory.AllBlockstates.Count));
			}

			_bits = bits;

			int pIndex = 0;
			foreach (var p in palette.Cast<NbtCompound>())
			{
				string name = p["Name"].StringValue;

				var blockState = BlockFactory.GetBlockState(name);

				if (p.TryGet<NbtCompound>("Properties", out NbtCompound properties))
				{
					foreach (var property in properties)
					{
						blockState = blockState.WithProperty(StateProperty.Parse(property.Name), property.StringValue);
					}
				}

				Palette.IdFor(blockState);
				//pIndex++;
			}
		}

		public void Read(MinecraftStream ms)
		{
			int bitsPerBlock = ms.ReadByte();
			if (this._bits != bitsPerBlock)
			{
				SetBits(bitsPerBlock);
			}

			Palette.Read(ms);

			var backingArray = Storage._data;

			int length = ms.ReadVarInt();

			if (backingArray == null || backingArray.Length != length)
			{
				backingArray = new long[length];
			}

			for (int j = 0; j < backingArray.Length; j++)
			{
				backingArray[j] = ms.ReadLong();
			}
		}
	}
}
