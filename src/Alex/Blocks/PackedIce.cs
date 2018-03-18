using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class PackedIce : Block
	{
		public PackedIce() : base(6751)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.PackedIce;
		}
	}
}
