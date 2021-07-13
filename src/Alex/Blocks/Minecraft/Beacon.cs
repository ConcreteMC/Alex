using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Beacon : Block
	{
		public Beacon() : base()
		{
			Solid = true;
			Transparent = true;
			Luminance = 15;
			CanInteract = true;
			
			//Hardness = 3;

			BlockMaterial = Material.Glass.Clone().WithHardness(3f);
		}
	}
}
