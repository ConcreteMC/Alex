using System.Collections.Generic;
using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Pallete
{
	public class BlockStatePaletteMap : IBlockStatePalette
	{
		private IBlockStatePaletteResizer _paletteResizer;
		private int _bits;

		private Dictionary<uint, IBlockState> _map;
		private Dictionary<int, uint> _byValue;

		private uint nextIndex = 0;
		public BlockStatePaletteMap(int bitsIn, IBlockStatePaletteResizer paletteResizerIn)
		{
			this._bits = bitsIn;
			this._paletteResizer = paletteResizerIn;

			_map = new Dictionary<uint, IBlockState>(1 << bitsIn);
			_byValue = new Dictionary<int, uint>();
		}

		public uint IdFor(IBlockState state)
		{
			if (state == null) return uint.MaxValue;
			var hash = state.GetHashCode();

			if (!_byValue.TryGetValue(hash, out uint index))
			{
				index = nextIndex;
				nextIndex++;

				if (index >= 1 << this._bits)
				{
					index = this._paletteResizer.OnResize(this._bits + 1, state);
				}
				else
				{
					_byValue.Add(hash, index);
					_map.Add(index, state);
				}
			}

			return index;
		}

		public IBlockState GetBlockState(uint indexKey)
		{
			return indexKey >= 0 && indexKey < _map.Count ? _map[indexKey] : default(IBlockState);
		}

		public int GetSerializedSize()
		{
			int i = BlockState.GetVarIntSize(this._map.Count);

			for (uint j = 0; j < this._map.Count; ++j)
			{
				i += BlockState.GetVarIntSize(
					BlockFactory.GetBlockStateId(_map[j]));
			}

			return i;
		}
	}
}