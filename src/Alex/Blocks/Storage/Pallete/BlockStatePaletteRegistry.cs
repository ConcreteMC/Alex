using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;

namespace Alex.Blocks.Storage.Pallete
{
    public class BlockStatePaletteRegistry : IBlockStatePalette
    {
	    public uint IdFor(IBlockState state)
	    {
		    uint i = state.ID;
		    return i == uint.MaxValue ? 0 : i;
		}

	    public IBlockState GetBlockState(uint indexKey)
	    {
			IBlockState iblockstate = BlockFactory.GetBlockState(indexKey);// .getByValue(indexKey);
		    return iblockstate == null ? new Air().GetDefaultState() : iblockstate;
		}

	    public void Read(IMinecraftStream ms)
	    {
		    ms.ReadVarInt();
	    }

	    public int GetSerializedSize()
	    {
		    return 0;
	    }
    }
}
