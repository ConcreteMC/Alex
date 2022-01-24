using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft.Doors;

namespace Alex.Blocks.Minecraft
{
	public class JungleDoor : Door
	{
		public JungleDoor() : base(7726)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Wood;
		}
	}
}