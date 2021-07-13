using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Torch : Block
	{
		public Torch(bool wallTorch = false) : base()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;
			
			Luminance = 14;

			BlockMaterial = Material.Circuits;
		}
	}
}
