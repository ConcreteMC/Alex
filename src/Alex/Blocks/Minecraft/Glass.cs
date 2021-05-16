using Alex.API.Blocks;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class Glass : Block
	{
		public Glass() : base()
		{
			Solid = true;
			Transparent = true;
			IsFullCube = true;

			BlockMaterial = Material.Glass;
		}
	}

	public class StainedGlass : Glass
	{
		public StainedGlass()
		{
			
		}
	}
}
