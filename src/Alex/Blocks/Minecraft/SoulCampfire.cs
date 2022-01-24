using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class SoulCampfire : Block
	{
		public SoulCampfire()
		{
			Luminance = 10;

			base.BlockMaterial = Material.Wood;
		}
	}
}