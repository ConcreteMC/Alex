using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class DeadBush : Block
	{
		public DeadBush() : base()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Plants;
		}
	}
}