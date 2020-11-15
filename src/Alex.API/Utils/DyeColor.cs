using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
	public class DyeColor
	{
		public int    Id          { get; }
		public string Description { get; }
		public Color  Color       { get; }

		public DyeColor(int id, string description, string hex)
		{
			Id = id;
			Description = description;
			Color = ColorHelper.HexToColor(hex);
		}

		public static readonly DyeColor InkSac       = new DyeColor(0, "Ink Sac", "#1D1D21");
		public static readonly DyeColor RedDye       = new DyeColor(1, "Red Dye", "#B02E26");
		public static readonly DyeColor GreenDye     = new DyeColor(2, "Green Dye", "#5E7C16");
		public static readonly DyeColor CocoaBeans   = new DyeColor(3, "Cocoa Beans", "#835432");
		public static readonly DyeColor LapisLazuli  = new DyeColor(4, "Lapis Lazuli", "#3C44AA");
		public static readonly DyeColor PurpleDye    = new DyeColor(5, "Purple Dye", "#8932B8");
		public static readonly DyeColor CyanDye      = new DyeColor(6, "Cyan Dye", "#169C9C");
		public static readonly DyeColor LightGrayDye = new DyeColor(7, "Light Gray Dye", "#9D9D97");
		public static readonly DyeColor GrayDye      = new DyeColor(8, "Gray Dye", "#474F52");
		public static readonly DyeColor PinkDye      = new DyeColor(9, "Pink Dye", "#F38BAA");
		public static readonly DyeColor LimeDye      = new DyeColor(10, "Lime Dye", "#80C71F");
		public static readonly DyeColor YellowDye    = new DyeColor(11, "Yellow Dye", "#FED83D");
		public static readonly DyeColor LightBlueDye = new DyeColor(12, "Light Blue Dye", "#3AB3DA");
		public static readonly DyeColor MagentaDye   = new DyeColor(13, "Magenta Dye", "#C74EBD");
		public static readonly DyeColor OrangeDye    = new DyeColor(14, "Orange Dye", "#F9801D");
		public static readonly DyeColor BoneMeal     = new DyeColor(15, "Bone Meal", "#F9FFFE");
		public static readonly DyeColor BlackDye     = new DyeColor(16, "Black Dye", "#1D1D21");
		public static readonly DyeColor BrownDye     = new DyeColor(17, "Brown Dye", "#835432");
		public static readonly DyeColor BlueDye      = new DyeColor(18, "Blue Dye", "#3C44AA");
		public static readonly DyeColor WhiteDye     = new DyeColor(19, "White Dye", "#F9FFFE");

		private static readonly IDictionary<int, DyeColor> Mappings = new Dictionary<int, DyeColor>();
		static DyeColor()
		{
			var t = typeof(DyeColor);
			foreach (var property in t.GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				if (!t.IsAssignableFrom(property.FieldType))
					continue;

				var color = property.GetValue(null) as DyeColor;
				if (color == null) continue;
				
				Mappings.TryAdd(color.Id, color);
			}
		}
		
		public static DyeColor FromId(int id)
		{
			if (Mappings.TryGetValue(id, out var color))
				return color;
			
			throw new Exception("The specified color id is not registered!");
		}
	}
}