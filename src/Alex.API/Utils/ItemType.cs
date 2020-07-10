using System;

namespace Alex.API.Utils
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
	
	[Flags]
	public enum ItemMaterial : int
	{
		Any = None | AnyMaterial,
		AnyMaterial = Wood | Stone | Gold | Iron | Diamond,
		//Leather = -2, // 0xFFFFFFFE
		//Chain = -1, // 0xFFFFFFFF
		
		None = 1 << 0,
		Wood = 1 << 1,
		Stone = 1 << 2,
		Gold = 1 << 3,
		Iron = 1 << 4,
		Diamond = 1 << 5,
	}
}
