using Alex.API.Blocks.State;

namespace Alex.Blocks.Storage
{
    public class DirectPallete : IPallete<IBlockState>
    {
        public uint GetId(IBlockState state)
        {
            return state.ID;
        }

        public uint Add(IBlockState state)
        {
            throw new System.NotImplementedException();
        }

        public IBlockState Get(uint id)
        {
            return BlockFactory.GetBlockState(id);
        }

        public void Put(IBlockState objectIn, uint intKey)
        {
            
        }
    }
}