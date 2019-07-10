using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using NLog;

namespace Alex.Blocks.Storage.Pallete
{
	public class BlockStatePaletteLinear : IBlockStatePalette
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockStatePaletteLinear));

        private readonly IBlockState[] _states;
		private readonly IBlockStatePaletteResizer _resizeHandler;
		private readonly int _bits;
		private uint _arraySize;

		public BlockStatePaletteLinear(int bitsIn, IBlockStatePaletteResizer resizeHandlerIn)
		{
			this._states = new IBlockState[1 << bitsIn];
			this._bits = bitsIn;
			this._resizeHandler = resizeHandlerIn;
		}

		public uint IdFor(IBlockState state)
		{
			for (uint i = 0; i < this._arraySize; i++)
			{
				if (this._states[i].ExactMatch(state))
				{
					return i;
				}
			}

			uint j = this._arraySize;

			if (j < this._states.Length)
			{
				this._states[j] = state;
				_arraySize++;
				return j;
			}
			else
			{
				return this._resizeHandler.OnResize(this._bits + 1, state);
			}
		}

		public IBlockState GetBlockState(uint indexKey)
		{
			return indexKey < this._arraySize ? this._states[indexKey] : null;
		}

		public void Read(IMinecraftStream ms)
		{
			this._arraySize = (uint) ms.ReadVarInt();

            for (int i = 0; i < _arraySize; i++)
            {
	            var state = BlockFactory.GetBlockState(ms.ReadVarInt());
					
                _states[i] = state;
			}
		}
	}
}
