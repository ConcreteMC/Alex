namespace Alex.Blocks.Minecraft
{
	public class PumpkinStem : Block
	{
		public PumpkinStem() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Plants;
		}
	}
}
