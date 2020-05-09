using Alex.API.Blocks.State;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage
{
    public class DirectPallete : IPallete<BlockState>
    {
        public uint GetId(BlockState state)
        {
            return state.ID;
        }

        public uint Add(BlockState state)
        {
            throw new System.NotImplementedException();
        }

        public BlockState Get(uint id)
        {
            return BlockFactory.GetBlockState(id);
        }

        public void Put(BlockState objectIn, uint intKey)
        {
            
        }
    }
}