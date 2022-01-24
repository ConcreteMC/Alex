using System;
using Alex.Common.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public class MapColor : IMapColor
	{
		private static readonly MapColor[] BaseColors = new MapColor[64];
		private static readonly Color[] BlockColors = new Color[256];

		public static readonly MapColor Air = new(0, 0, 0, 0, 0);
		public static readonly MapColor Grass = new(1, 127, 178, 56);
		public static readonly MapColor Sand = new(2, 247, 233, 163);
		public static readonly MapColor Cloth = new(3, 199, 199, 199);
		public static readonly MapColor Fire = new(4, 255, 0, 0);
		public static readonly MapColor Ice = new(5, 160, 160, 255);
		public static readonly MapColor Iron = new(6, 167, 167, 167);
		public static readonly MapColor Foliage = new(7, 0, 124, 0);
		public static readonly MapColor Snow = new(8, 255, 255, 255);
		public static readonly MapColor Clay = new(9, 164, 168, 184);
		public static readonly MapColor Dirt = new(10, 151, 109, 77);
		public static readonly MapColor Stone = new(11, 112, 112, 112);
		public static readonly MapColor Water = new(12, 64, 64, 255, 64);
		public static readonly MapColor Wood = new(13, 143, 119, 72);
		public static readonly MapColor Quartz = new(14, 255, 252, 245);
		public static readonly MapColor Orange = new(15, 216, 127, 51);

		public static readonly MapColor Magenta = new(16, 178, 76, 216);
		public static readonly MapColor LightBlue = new(17, 102, 153, 216);
		public static readonly MapColor Yellow = new(18, 229, 229, 51);
		public static readonly MapColor LightGreen = new(19, 127, 204, 25);
		public static readonly MapColor Pink = new(20, 242, 127, 165);
		public static readonly MapColor Gray = new(21, 76, 76, 76);
		public static readonly MapColor LightGray = new(22, 153, 153, 153);
		public static readonly MapColor Cyan = new(23, 76, 127, 153);
		public static readonly MapColor Purple = new(24, 127, 63, 178);
		public static readonly MapColor Blue = new(25, 51, 76, 178);
		public static readonly MapColor Brown = new(26, 102, 76, 51);
		public static readonly MapColor Green = new(27, 102, 127, 51);
		public static readonly MapColor Red = new(28, 153, 51, 51);
		public static readonly MapColor Black = new(29, 25, 25, 25);
		public static readonly MapColor Gold = new(30, 250, 238, 77);

		public static readonly MapColor Diamond = new(31, 92, 219, 213);
		public static readonly MapColor Lapis = new(32, 74, 128, 255);
		public static readonly MapColor Emerald = new(33, 0, 217, 58);
		public static readonly MapColor Podzol = new(34, 129, 86, 49);
		public static readonly MapColor Nether = new(35, 112, 2, 0);

		public static readonly MapColor TerracottaWhite = new(36, 209, 177, 161);
		public static readonly MapColor TerracottaOrange = new(37, 159, 82, 36);
		public static readonly MapColor TerracottaMagenta = new(38, 149, 87, 108);
		public static readonly MapColor TerracottaLightBlue = new(39, 112, 108, 138);
		public static readonly MapColor TerracottaYellow = new(40, 186, 133, 36);
		public static readonly MapColor TerracottaLightGreen = new(41, 103, 117, 53);
		public static readonly MapColor TerracottaPink = new(42, 160, 77, 78);
		public static readonly MapColor TerracottaGray = new(43, 57, 41, 35);
		public static readonly MapColor TerracottaLightGray = new(44, 135, 107, 98);
		public static readonly MapColor TerracottaCyan = new(45, 87, 92, 92);
		public static readonly MapColor TerracottaPurple = new(46, 122, 73, 88);
		public static readonly MapColor TerracottaBlue = new(47, 76, 62, 92);
		public static readonly MapColor TerracottaBrown = new(48, 76, 50, 35);
		public static readonly MapColor TerracottaGreen = new(49, 76, 82, 42);
		public static readonly MapColor TerracottaRed = new(50, 142, 60, 46);
		public static readonly MapColor TerracottaBlack = new(51, 37, 22, 16);

		public static readonly MapColor CrimsonNylium = new(52, 189, 48, 49);
		public static readonly MapColor CrimsonStem = new(53, 148, 63, 97);
		public static readonly MapColor CrimsonHyphae = new(54, 92, 25, 29);

		public static readonly MapColor WarpedNylium = new(55, 22, 126, 134);
		public static readonly MapColor WarpedStem = new(56, 58, 142, 140);
		public static readonly MapColor WarpedHyphae = new(57, 86, 44, 62);
		public static readonly MapColor WarpedWartBlock = new(58, 20, 180, 133);

		public Color BaseColor { get; }
		public int Index { get; }

		private MapColor(int index, byte r, byte g, byte b, byte a = 255)
		{
			if (index >= 0 && index <= (BaseColors.Length - 1))
			{
				Index = index;
				BaseColor = new Color(r, g, b, a);
				BaseColors[index] = this;

				BlockColors[index * 4 + 0] = GetMapColor(0);
				BlockColors[index * 4 + 1] = GetMapColor(1);
				BlockColors[index * 4 + 2] = BaseColor;
				BlockColors[index * 4 + 3] = GetMapColor(3);
			}
			else
			{
				throw new IndexOutOfRangeException("Map colour ID must be between 0 and 63 (inclusive)");
			}
		}

		//See https://minecraft.fandom.com/wiki/Map_item_format
		public Color GetMapColor(int index)
		{
			int i = index switch
			{
				0 => 180,
				1 => 220,
				2 => 255,
				3 => 135,
				_ => 220
			};

			var modifier = i / 255f;

			return new Color(
				(byte)(BaseColor.R * modifier), (byte)(BaseColor.G * modifier), (byte)(BaseColor.B * modifier),
				BaseColor.A);
		}

		/// <inheritdoc />
		public IMapColor WithAlpha(byte alpha)
		{
			return new MapColor(Index, BaseColor.R, BaseColor.G, BaseColor.B, alpha);
		}

		public static IMapColor GetBaseColor(int index)
		{
			return BaseColors[index];
		}

		public static Color GetBlockColor(int index)
		{
			return BlockColors[index];
		}
	}
}