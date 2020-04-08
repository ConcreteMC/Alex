namespace Alex.Blocks.Minecraft
{
	public class PumpkinStem : Block
	{
		public PumpkinStem() : base(4162)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Plants;
		}
	}
}
