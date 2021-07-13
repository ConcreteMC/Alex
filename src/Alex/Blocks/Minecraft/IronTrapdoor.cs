using Alex.Blocks.Materials;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Minecraft
{
	public class IronTrapdoor : Trapdoor
	{
		public IronTrapdoor() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial  = Material.Iron;
		}
	}
}
