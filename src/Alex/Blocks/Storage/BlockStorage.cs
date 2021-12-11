using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alex.Blocks.State;
using Alex.Blocks.Storage.Palette;

namespace Alex.Blocks.Storage
{
    public class BlockStorage : GenericStorage<BlockState>
    {
        private BlockState Air { get; }
        public BlockStorage() : base(BlockFactory.GetBlockState("minecraft:air"))
        {
            Air = BlockFactory.GetBlockState("minecraft:air");
            X = Y = Z = 16;
        }

        /// <inheritdoc />
        protected override BlockState GetDefault()
        {
            return Air;
        }

        /// <inheritdoc />
        protected override int GetIndex(int x, int y, int z)
        {
            return y << 8 | z << 4 | x;
        }

        /// <inheritdoc />
        protected override int CalculateDirectPaletteSize()
        {
            return (int)Math.Ceiling(Math.Log2(BlockFactory.AllBlockstates.Count));
        }

        /// <inheritdoc />
        protected override DirectPallete<BlockState> GetDirectPalette()
        {
            return new DirectPallete<BlockState>(GlobalLookup);
        }

        private static BlockState GlobalLookup(uint arg)
        {
            return BlockFactory.GetBlockState(arg);
        }
    }
}