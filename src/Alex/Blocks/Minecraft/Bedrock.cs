using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Bedrock : Block
	{
		public Bedrock() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.Clone().WithHardness(60000);
		}
	}
}
