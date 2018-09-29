using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Anvil : Block
	{
		public Anvil() : base(5477)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Anvil;
		}
	}
}
