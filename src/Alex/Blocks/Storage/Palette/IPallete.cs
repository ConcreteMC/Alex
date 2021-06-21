using System;
using Alex.Blocks.State;

namespace Alex.Blocks.Storage.Palette
{
    public interface IPallete : IDisposable
    {
        uint GetId(BlockState state);

        uint Add(BlockState state);

        BlockState Get(uint id);

        void Put(BlockState objectIn, uint intKey);
    }
}