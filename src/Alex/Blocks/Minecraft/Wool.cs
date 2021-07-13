using Alex.Blocks.Materials;
using Alex.Common.Utils;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft
{
	public class Wool : Block
	{
		public Wool(BedColor color)
		{
			BlockMaterial = Material.Wool.Clone().WithMapColor(color.ToMapColor());
		}
	}
}