using Alex.Blocks;
using Alex.Graphics.Items;

namespace Alex
{
    public static class BlockFactory
    {
        public static Block GetBlock(byte id, byte metadata)
        {
            if (id == 0) return new Air();
            if (id == 1) return new Stone(metadata);
            if (id == 2) return new Grass();
            if (id == 3) return new Dirt();
			if (id == 5) return new Planks(metadata);
            if (id == 7) return new Bedrock();
	        if (id == 8 || id == 9) return new Water();// return new Air();
			if (id == 12) return new Sand(metadata);
			if (id == 17) return new Wood(metadata);
			if (id == 18) return new Leaves(metadata);
			if (id == 20) return new Glass();
			if (id == 31) return new TallGrass();
			if (id == 41) return new GoldBlock();
			if (id == 42) return new IronBlock();
			if (id == 44) return new StoneSlab();
            //if (id == 43 && metadata == 5) return new StoneBrick();
            if (id == 43) return new StoneBrickSlab();
            if (id == 50) return new Torch();
            if (id == 89) return new GlowStone();
			if (id == 95) return new InvisibleBedrock(); // Invisible Bedrock
			//if (id == 95) return new StainedGlass(metadata);
            if (id == 98) return new StoneBrick();
            if (id == 101) return new IronBars();
			if (id == 121) return new EndStone();
            if (id == 159) return new StainedClay(metadata);
            if (id == 161) return new AcaciaLeaves();
			if (id == 172) return new HardenenedClay();
			if (id == 173) return new CoalBlock();

			return new Block(id, metadata);
        }
    }
}
