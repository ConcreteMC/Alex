using Alex.Blocks.Materials;
using Alex.Common.Utils;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft
{
	public class Concrete : Block
	{
		public Concrete(BedColor color)
		{
			base.BlockMaterial = Material.Clay.Clone().WithMapColor(color.ToMapColor());
		}
	}
	
	public class ConcretePowder : Block
	{
		public ConcretePowder(BedColor color)
		{
			base.BlockMaterial = Material.Sand.Clone().WithMapColor(color.ToMapColor());
		}
	}
}