using Alex.API.Items;
using Alex.API.Utils;
using Alex.Items;

namespace Alex.Blocks.Minecraft
{
	public class Stone : Block
	{
		public Stone() : base(1)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 1.5f;
		}

		public override IItem[] GetDrops(IItem tool)
		{
			if (tool.ItemType != ItemType.PickAxe)
				return new IItem[0];
			
			return new IItem[]
			{
				new ItemBlock(BlockFactory.GetBlockState("minecraft:cobblestone"))
				{
					Count = 1
				}, 
			};
		}
	}
}
