using Alex.Blocks.Materials;
using Alex.Common.Utils;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft
{
	public class Wool : Block
	{
		public Wool(BlockColor color)
		{
			BlockMaterial = Material.Wool.Clone().WithMapColor(color.ToMapColor());
		}
	}
}