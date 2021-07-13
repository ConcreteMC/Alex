using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class InvisibleBedrock : Block
	{
		public InvisibleBedrock(bool pe = true) : base()
		{
			Renderable = false;
			HasHitbox = false;
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Portal.Clone().WithHardness(60000);
			//Hardness = 60000;
		}
	}
}
