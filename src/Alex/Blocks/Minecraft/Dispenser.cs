using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Dispenser : Block
	{
		public Dispenser() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Decoration;
			
			CanInteract = true;
		}
	}
}
