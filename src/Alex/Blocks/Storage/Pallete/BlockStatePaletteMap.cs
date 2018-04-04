using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Pallete
{
	public class BlockStatePaletteMap : IBlockStatePalette
	{
		private IBlockStatePaletteResizer _paletteResizer;
		private int _bits;

	//	private Dictionary<uint, IBlockState> _map;
		//private Dictionary<int, uint> _byValue;
		private IntIdentityHashBiMap<IBlockState> _map;

		private uint nextIndex = 0;
		public BlockStatePaletteMap(int bitsIn, IBlockStatePaletteResizer paletteResizerIn)
		{
			this._bits = bitsIn;
			this._paletteResizer = paletteResizerIn;

			_map = new IntIdentityHashBiMap<IBlockState>(1 << bitsIn);

		//	_map = new Dictionary<uint, IBlockState>(1 << bitsIn);
		//	_byValue = new Dictionary<int, uint>(1 << bitsIn);
		}

		public uint IdFor(IBlockState state)
		{
			uint i = _map.GetId(state);

			if (i == uint.MaxValue)
			{
				i = _map.Add(state);

				if (i >= 1 << this._bits)
				{
					i = this._paletteResizer.OnResize(_bits + 1, state);
				}
			}

			return i;
		}

		/*(public uint IdFor(IBlockState state)
		{
			if (state == null) return uint.MaxValue;

			var hash = RuntimeHelpers.GetHashCode(state.ID);
			
			if (!_byValue.TryGetValue(hash, out uint index))
			{
				index = nextIndex;
				nextIndex++;

				if (index >= (1 << this._bits))
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
		}*/

		public IBlockState GetBlockState(uint indexKey)
		{
			return _map.Get(indexKey);
			//return indexKey >= 0 && indexKey < _map.Count ? _map[indexKey] : default(IBlockState);
		}

		public void Read(IMinecraftStream ms)
		{
			_map.Clear();
			int size = ms.ReadVarInt();

			for (int i = 0; i < size; ++i)
			{
				_map.Add(BlockFactory.GetBlockState(ms.ReadVarInt()));
			}
		}
	}
}