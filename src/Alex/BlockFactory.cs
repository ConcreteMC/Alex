using Alex.Blocks;
using Alex.Graphics.Items;

namespace Alex
{
    public static class BlockFactory
    {
        public static Block GetBlock(ushort id, byte metadata)
        {
            if (id == 0) return new Air();
            if (id == 1) return new Stone(metadata);
            if (id == 2) return new Grass();
            if (id == 3) return new Dirt();
			if (id == 5) return new Planks(metadata);
            if (id == 7) return new Bedrock();
            if (id == 8 || id == 9) return new Water();
			if (id == 17) return new Wood(metadata);
			if (id == 18) return new Leaves(metadata);
			if (id == 20) return new Glass();
			if (id == 44 && metadata == 0) return new StoneSlab();
            if (id == 43 && metadata == 5) return new StoneBrick();
            if (id == 43 && metadata == 5) return new StoneBrickSlab();
            if (id == 50) return new Torch();
            if (id == 89) return new GlowStone();
			if (id == 95) return new Air(); // Invisible Bedrock
			//if (id == 95) return new StainedGlass(metadata);
            if (id == 98) return new StoneBrick();
            if (id == 101) return new IronBars();
			if (id == 121) return new EndStone();
            if (id == 159) return new StainedClay(metadata);

			return new Stone();
        }
    }
}
