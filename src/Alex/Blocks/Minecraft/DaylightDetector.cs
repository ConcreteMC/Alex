namespace Alex.Blocks.Minecraft
{
	public class DaylightDetector : Block
	{
		public DaylightDetector() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Circuits;
		}
	}
}
