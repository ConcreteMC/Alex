using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class EndRod : Block
	{
		public EndRod() : base()
		{
			Solid = true;
			Transparent = true;
			Luminance = 14;

			BlockMaterial = Material.Glass;
			IsFullCube = false;
		}
	}
}