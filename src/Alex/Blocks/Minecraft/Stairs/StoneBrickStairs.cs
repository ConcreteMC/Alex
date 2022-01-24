using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class StoneBrickStairs : Stairs
	{
		public StoneBrickStairs() : base(4333)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}