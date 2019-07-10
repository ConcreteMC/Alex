using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;

namespace Alex.Blocks.Storage.Pallete
{
	public class BlockStatePaletteMap : IBlockStatePalette
	{
		private IBlockStatePaletteResizer _paletteResizer;
		private int _bits;

		private IntIdentityHashBiMap<IBlockState> _map;


		public BlockStatePaletteMap(int bitsIn, IBlockStatePaletteResizer paletteResizerIn)
		{
			this._bits = bitsIn;
			this._paletteResizer = paletteResizerIn;

			_map = new IntIdentityHashBiMap<IBlockState>(1 << bitsIn);
		}

		public uint IdFor(IBlockState state)
		{
			uint i = _map.GetId(state);

			if (i == uint.MaxValue)
			{
				i = _map.Add(state);

				if (i >= 1 << this._bits)
				{
					return this._paletteResizer.OnResize(_bits + 1, state);
				}
			}

			return i;
		}

		public IBlockState GetBlockState(uint indexKey)
		{
			return _map.Get(indexKey);
		}

		public void Read(IMinecraftStream ms)
		{
			_map.Clear();
			int size = ms.ReadVarInt();

			for (int i = 0; i < size; i++)
			{
				var id = ms.ReadVarInt();
				var blockstate = BlockFactory.GetBlockState(id);
				_map.Add(blockstate);
			}
		}
	}
}