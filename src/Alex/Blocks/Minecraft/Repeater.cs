using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft.Decorations;

namespace Alex.Blocks.Minecraft
{
	public class Repeater : RedstoneBase
	{
		public Repeater()
		{
			BlockMaterial = Material.Decoration;
			CanInteract = true;
		}
	}
}