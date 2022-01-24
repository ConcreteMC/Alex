using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Dropper : Block
	{
		public Dropper() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Decoration.Clone().WithHardness(3.5f);

			CanInteract = true;
		}
	}
}