using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Walls
{
	public class AndesiteWall : AbstractWall
	{
		public AndesiteWall()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;			
		}
	}
}