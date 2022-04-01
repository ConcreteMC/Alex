using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft.Decorations;

namespace Alex.Blocks.Minecraft.Buttons
{
	public class Button : RedstoneBase
	{
		public Button(uint id) : base()
		{
			Transparent = true;
			Solid = false;
			IsFullCube = false;
			CanInteract = true;

			BlockMaterial = Material.Decoration;
			/*
			 * Solid = false;
			Transparent = true;
			IsReplacible = false;
			 */
		}
	}
}