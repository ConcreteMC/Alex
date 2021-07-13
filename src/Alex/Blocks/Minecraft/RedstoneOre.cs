using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class RedstoneOre : Block
	{
		public RedstoneOre() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Ore;
		}
		
		public override byte Luminance {
			get
			{
				if (Lit.GetValue(BlockState))
				{
					return 9;
				}

				return 0;
			} 
		}
	}
}
