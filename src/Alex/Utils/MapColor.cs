using System;
using Alex.API.Blocks;

namespace Alex.Utils
{
	public class MapColor : IMapColor
	{
		public static MapColor[] COLORS = new MapColor[64];
		public static MapColor[] BLOCK_COLORS = new MapColor[16];
		public static MapColor AIR = new MapColor(0, 0);
		public static MapColor GRASS = new MapColor(1, 8368696);
		public static MapColor SAND = new MapColor(2, 16247203);
		public static MapColor CLOTH = new MapColor(3, 13092807);
		public static MapColor TNT = new MapColor(4, 16711680);
		public static MapColor ICE = new MapColor(5, 10526975);
		public static MapColor IRON = new MapColor(6, 10987431);
		public static MapColor FOLIAGE = new MapColor(7, 31744);
		public static MapColor SNOW = new MapColor(8, 16777215);
		public static MapColor CLAY = new MapColor(9, 10791096);
		public static MapColor DIRT = new MapColor(10, 9923917);
		public static MapColor STONE = new MapColor(11, 7368816);
		public static MapColor WATER = new MapColor(12, 4210943);
		public static MapColor WOOD = new MapColor(13, 9402184);
		public static MapColor QUARTZ = new MapColor(14, 16776437);
		public static MapColor ADOBE = new MapColor(15, 14188339);
		public static MapColor MAGENTA = new MapColor(16, 11685080);
		public static MapColor LIGHT_BLUE = new MapColor(17, 6724056);
		public static MapColor YELLOW = new MapColor(18, 15066419);
		public static MapColor LIME = new MapColor(19, 8375321);
		public static MapColor PINK = new MapColor(20, 15892389);
		public static MapColor GRAY = new MapColor(21, 5000268);
		public static MapColor SILVER = new MapColor(22, 10066329);
		public static MapColor CYAN = new MapColor(23, 5013401);
		public static MapColor PURPLE = new MapColor(24, 8339378);
		public static MapColor BLUE = new MapColor(25, 3361970);
		public static MapColor BROWN = new MapColor(26, 6704179);
		public static MapColor GREEN = new MapColor(27, 6717235);
		public static MapColor RED = new MapColor(28, 10040115);
		public static MapColor BLACK = new MapColor(29, 1644825);
		public static MapColor GOLD = new MapColor(30, 16445005);
		public static MapColor DIAMOND = new MapColor(31, 6085589);
		public static MapColor LAPIS = new MapColor(32, 4882687);
		public static MapColor EMERALD = new MapColor(33, 55610);
		public static MapColor OBSIDIAN = new MapColor(34, 8476209);
		public static MapColor NETHERRACK = new MapColor(35, 7340544);
		public static MapColor WHITE_STAINED_HARDENED_CLAY = new MapColor(36, 13742497);
		public static MapColor ORANGE_STAINED_HARDENED_CLAY = new MapColor(37, 10441252);
		public static MapColor MAGENTA_STAINED_HARDENED_CLAY = new MapColor(38, 9787244);
		public static MapColor LIGHT_BLUE_STAINED_HARDENED_CLAY = new MapColor(39, 7367818);
		public static MapColor YELLOW_STAINED_HARDENED_CLAY = new MapColor(40, 12223780);
		public static MapColor LIME_STAINED_HARDENED_CLAY = new MapColor(41, 6780213);
		public static MapColor PINK_STAINED_HARDENED_CLAY = new MapColor(42, 10505550);
		public static MapColor GRAY_STAINED_HARDENED_CLAY = new MapColor(43, 3746083);
		public static MapColor SILVER_STAINED_HARDENED_CLAY = new MapColor(44, 8874850);
		public static MapColor CYAN_STAINED_HARDENED_CLAY = new MapColor(45, 5725276);
		public static MapColor PURPLE_STAINED_HARDENED_CLAY = new MapColor(46, 8014168);
		public static MapColor BLUE_STAINED_HARDENED_CLAY = new MapColor(47, 4996700);
		public static MapColor BROWN_STAINED_HARDENED_CLAY = new MapColor(48, 4993571);
		public static MapColor GREEN_STAINED_HARDENED_CLAY = new MapColor(49, 5001770);
		public static MapColor RED_STAINED_HARDENED_CLAY = new MapColor(50, 9321518);
		public static MapColor BLACK_STAINED_HARDENED_CLAY = new MapColor(51, 2430480);
		public int colorValue;
		public int colorIndex;

		private MapColor(int index, int color)
		{
			if (index >= 0 && index <= 63)
			{
				this.colorIndex = index;
				this.colorValue = color;
				COLORS[index] = this;
			}
			else
			{
				throw new IndexOutOfRangeException("Map colour ID must be between 0 and 63 (inclusive)");
			}
		}

		public int GetMapColor(int index)
		{
			int i = 220;

			if (index == 3)
			{
				i = 135;
			}

			if (index == 2)
			{
				i = 255;
			}

			if (index == 1)
			{
				i = 220;
			}

			if (index == 0)
			{
				i = 180;
			}

			int j = (this.colorValue >> 16 & 255) * i / 255;
			int k = (this.colorValue >> 8 & 255) * i / 255;
			int l = (this.colorValue & 255) * i / 255;
			return -16777216 | j << 16 | k << 8 | l;
		}
	}
}
