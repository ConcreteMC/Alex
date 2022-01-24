using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class JackOLantern : Block
	{
		public JackOLantern()
		{
			Solid = true;
			Transparent = false;

			Luminance = 15;

			BlockMaterial = Material.Wood.Clone().WithHardness(1);
		}
	}
}