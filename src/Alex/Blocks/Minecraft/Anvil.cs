using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Anvil : Block
	{
		public Anvil() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Anvil;
			
			CanInteract = true;
		}
	}
}
