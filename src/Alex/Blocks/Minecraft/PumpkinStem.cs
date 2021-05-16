namespace Alex.Blocks.Minecraft
{
	public class PumpkinStem : Block
	{
		public PumpkinStem() : base()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Plants;
		}
	}
}
