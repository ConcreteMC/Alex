namespace Alex.Blocks.Minecraft
{
	public class BrownMushroomBlock : Block
	{
		public BrownMushroomBlock() : base()
		{
			Solid = true;
			Transparent = false;
			
			BlockMaterial = Material.Ground.SetHardness(0.2f);
		}
	}
}
