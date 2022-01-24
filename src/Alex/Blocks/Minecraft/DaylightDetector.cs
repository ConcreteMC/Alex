using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class DaylightDetector : Block
	{
		public DaylightDetector() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Decoration;
		}
	}
}