using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class RedSandstoneStairs : Stairs
	{
		public RedSandstoneStairs() : base(7098)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}
