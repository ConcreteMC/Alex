using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Walls
{
	public class CobblestoneWall : AbstractWall
	{
		public CobblestoneWall() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}
