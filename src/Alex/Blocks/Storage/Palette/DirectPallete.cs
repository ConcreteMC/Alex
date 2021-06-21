using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Palette
{
    public class DirectPallete : IPallete
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

        /// <inheritdoc />
        public void Dispose() { }
    }
}