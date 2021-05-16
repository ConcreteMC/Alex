namespace Alex.Blocks.Minecraft
{
	public class BrownMushroom : Block
	{
		public BrownMushroom() : base()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Plants;
		}
	}
}
