namespace Alex.Blocks.Minecraft
{
	public class BrownMushroom : Block
	{
		public BrownMushroom() : base(1031)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Plants;
		}
	}
}
