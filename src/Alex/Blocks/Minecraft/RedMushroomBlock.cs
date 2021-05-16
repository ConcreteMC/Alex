namespace Alex.Blocks.Minecraft
{
	public class RedMushroomBlock : Block
	{
		public RedMushroomBlock() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Ground.SetHardness(0.2f);
		}
	}
}
