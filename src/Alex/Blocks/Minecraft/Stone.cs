using Alex.Common.Utils;
using Alex.Items;

namespace Alex.Blocks.Minecraft
{
	public class Stone : Block
	{
		public Stone() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.Clone().SetHardness(1.5f);
		}

		public override Item[] GetDrops(Item tool)
		{
			if (tool.ItemType != ItemType.PickAxe)
				return new Item[0];
			
			return new Item[]
			{
				new ItemBlock(BlockFactory.GetBlockState("minecraft:cobblestone"))
				{
					Count = 1
				}, 
			};
		}
	}
}
