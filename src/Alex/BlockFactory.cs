using Alex.Blocks;
using Alex.Graphics.Items;

namespace Alex
{
    public static class BlockFactory
    {
        public static Block GetBlock(ushort id, byte metadata)
        {
            if (id == 0) return new Air();
            if (id == 1) return new Stone();
            if (id == 2) return new Grass();
            if (id == 3) return new Dirt();
            if (id == 7) return new Bedrock();
			if (id == 17) return new Wood(metadata);
			if (id == 18) return new Leaves(metadata);
			if (id == 20) return new Glass();
			if (id == 43 && metadata == 0) return new StoneSlab();
            if (id == 50) return new Torch();
			if (id == 95) return new StainedGlass(metadata);

			return new Stone();
        }
    }
}
