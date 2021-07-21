using Alex.Blocks.Materials;
using Alex.Common.Utils;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft
{
	public class Concrete : Block
	{
		public Concrete(BlockColor color)
		{
			base.BlockMaterial = Material.Clay.Clone().WithMapColor(color.ToMapColor());
		}
	}
	
	public class ConcretePowder : Block
	{
		public ConcretePowder(BlockColor color)
		{
			base.BlockMaterial = Material.Sand.Clone().WithMapColor(color.ToMapColor());
		}
	}
}