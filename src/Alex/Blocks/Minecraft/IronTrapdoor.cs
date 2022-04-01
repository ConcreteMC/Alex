using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class IronTrapdoor : Trapdoor
	{
		public IronTrapdoor() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Metal;
		}
	}
}