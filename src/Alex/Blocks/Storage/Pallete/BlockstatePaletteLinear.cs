using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Pallete
{
	public class BlockStatePaletteLinear : IBlockStatePalette
	{
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
				if (this._states[i].ID == state.ID)
				{
					return i;
				}
			}

			uint j = (uint) this._arraySize;

			if (j < this._states.Length)
			{
				this._states[j] = state;
				++this._arraySize;
				return j;
			}
			else
			{
				return this._resizeHandler.OnResize(this._bits + 1, state);
			}
		}

		public IBlockState GetBlockState(uint indexKey)
		{
			return indexKey >= 0 && indexKey < this._arraySize ? this._states[indexKey] : null;
		}

		/*
			public void read(PacketBuffer buf)
			{
				this.arraySize = buf.readVarInt();

				for (int i = 0; i < this.arraySize; ++i)
				{
					this.states[i] = Block.BLOCK_STATE_IDS.getByValue(buf.readVarInt());
				}
			}

			public void write(PacketBuffer buf)
			{
				buf.writeVarInt(this.arraySize);

				for (int i = 0; i < this.arraySize; ++i)
				{
					buf.writeVarInt(Block.BLOCK_STATE_IDS.get(this.states[i]));
				}
			}
			*/
		public int GetSerializedSize()
		{
			int i = BlockState.GetVarIntSize(this._arraySize);

			for (int j = 0; j < this._arraySize; ++j)
			{
				i += BlockState.GetVarIntSize(
					BlockFactory.GetBlockStateId(_states[j])); /*Block.BLOCK_STATE_IDS.get(this._states[j]));*/
			}

			return i;
		}
	}
}
