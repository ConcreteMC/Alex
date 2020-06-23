using Alex.Blocks.State;

namespace Alex.Blocks.Storage
{
    public interface IPallete
    {
        uint GetId(BlockState state);

        uint Add(BlockState state);

        BlockState Get(uint id);

        void Put(BlockState objectIn, uint intKey);
    }
}