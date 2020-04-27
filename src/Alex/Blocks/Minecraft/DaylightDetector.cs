namespace Alex.Blocks.Minecraft
{
	public class DaylightDetector : Block
	{
		public DaylightDetector() : base(5577)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Circuits;
		}
	}
}
