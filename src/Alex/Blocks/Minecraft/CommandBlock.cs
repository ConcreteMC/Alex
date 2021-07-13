using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class CommandBlock : Block
	{
		public CommandBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;
			
			BlockMaterial  = Material.Metal;
		}
	}
}
