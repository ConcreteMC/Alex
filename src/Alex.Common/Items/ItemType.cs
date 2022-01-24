using System;

namespace Alex.Common.Items
{
	[Flags]
	public enum ItemType : int
	{
		Any = AnyTool | AnyArmor | Hand,
		AnyArmor = Helmet | Chestplate | Leggings | Boots,
		AnyTool = Sword | Shovel | PickAxe | Axe | Hoe | Shears,

		//Tools
		Sword = 1 << 0,
		Shovel = 1 << 1,
		PickAxe = 1 << 2,
		Axe = 1 << 3,
		Item = 1 << 4,
		Hoe = 1 << 5,
		Shears = 1 << 6,
		Hand = 1 << 7,
		Shield = 1 << 8,

		//Armor
		Helmet = 1 << 9,
		Chestplate = 1 << 10,
		Leggings = 1 << 11,
		Boots = 1 << 12
	}
}