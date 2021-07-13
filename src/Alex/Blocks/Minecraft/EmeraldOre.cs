using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class EmeraldOre : Block
	{
		public EmeraldOre() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Ore;//.Clone().WithMapColor(em);
		}
	}
}
