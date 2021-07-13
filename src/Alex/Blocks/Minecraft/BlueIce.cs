using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class BlueIce : Ice
	{
		public BlueIce()
		{
			Luminance = 4;

			BlockMaterial = Material.BlueIce;
		}
	}
}