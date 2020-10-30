namespace Alex.Blocks.Minecraft
{
	public class BrownMushroom : Block
	{
		public BrownMushroom() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Plants;
		}
	}
}
