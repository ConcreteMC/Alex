using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Pallete
{
	public class BlockStatePaletteHashMap : IBlockStatePalette
	{
		private IntIdentityHashBiMap<IBlockState> _statePaletteMap;
		private IBlockStatePaletteResizer _paletteResizer;
		private int _bits;

		public BlockStatePaletteHashMap(int bitsIn, IBlockStatePaletteResizer paletteResizerIn)
		{
			this._bits = bitsIn;
			this._paletteResizer = paletteResizerIn;
			this._statePaletteMap = new IntIdentityHashBiMap<IBlockState>(1 << bitsIn);
		}

		public int IdFor(IBlockState state)
		{
			int i = this._statePaletteMap.GetId(state);

			if (i == -1)
			{
				i = this._statePaletteMap.Add(state);

				if (i >= 1 << this._bits)
				{
					i = this._paletteResizer.OnResize(this._bits + 1, state);
				}
			}

			return i;
		}

		public IBlockState GetBlockState(int indexKey)
		{
			return _statePaletteMap.Get(indexKey);
		}

		/*@SideOnly(Side.CLIENT)
	
		public void read(PacketBuffer buf)
		{
			this.statePaletteMap.clear();
			int i = buf.readVarInt();
	
			for (int j = 0; j < i; ++j)
			{
				this.statePaletteMap.add(Block.BLOCK_STATE_IDS.getByValue(buf.readVarInt()));
			}
		}
	
		public void write(PacketBuffer buf)
		{
			int i = this.statePaletteMap.size();
			buf.writeVarInt(i);
	
			for (int j = 0; j < i; ++j)
			{
				buf.writeVarInt(Block.BLOCK_STATE_IDS.get(this.statePaletteMap.get(j)));
			}
		}*/

		public int GetSerializedSize()
		{
			int i = BlockState.GetVarIntSize(this._statePaletteMap.Size());

			for (int j = 0; j < this._statePaletteMap.Size(); ++j)
			{
				i += BlockState.GetVarIntSize(
					BlockFactory.GetBlockStateId(_statePaletteMap.Get(j)));
			}

			return i;
		}
	}
}