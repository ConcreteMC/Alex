using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils;

public class BannerColor
{
	public int Id { get; }
	public string Description { get; }
	public Color Color { get; }

	public BannerColor(int id, string description, string hex)
	{
		Id = id;
		Description = description;
		Color = ColorHelper.HexToColor(hex);
	}

	public static readonly BannerColor White = new BannerColor(0, "White", "#FFFFFF");
	public static readonly BannerColor Orange = new BannerColor(1, "Orange", "#D87F33");
	public static readonly BannerColor Magenta = new BannerColor(2, "Magenta", "#B24CD8");
	public static readonly BannerColor LightBlue = new BannerColor(3, "LightBlue", "#6699D8");
	public static readonly BannerColor Yellow = new BannerColor(4, "Yellow", "#E5E533");
	public static readonly BannerColor Lime = new BannerColor(5, "Lime", "#7FCC19");
	public static readonly BannerColor Pink = new BannerColor(6, "Pink", "#F27FA5");
	public static readonly BannerColor Gray = new BannerColor(7, "Gray", "#4C4C4C");
	public static readonly BannerColor LightGray = new BannerColor(8, "LightGray", "#999999");
	public static readonly BannerColor Cyan = new BannerColor(9, "Cyan", "#4C7F99");
	public static readonly BannerColor Purple = new BannerColor(10, "Purple", "#7F3FB2");
	public static readonly BannerColor Blue = new BannerColor(11, "Blue", "#334CB2");
	public static readonly BannerColor Brown = new BannerColor(12, "Brown", "#664C33");
	public static readonly BannerColor Green = new BannerColor(13, "Green", "#667F33");
	public static readonly BannerColor Red = new BannerColor(14, "Red", "#993333");
	public static readonly BannerColor Black = new BannerColor(15, "Black", "#191919");

	private static readonly IDictionary<int, BannerColor> Mappings = new Dictionary<int, BannerColor>();

	static BannerColor()
	{
		var t = typeof(BannerColor);

		foreach (var property in t.GetFields(BindingFlags.Static | BindingFlags.Public))
		{
			if (!t.IsAssignableFrom(property.FieldType))
				continue;

			var color = property.GetValue(null) as BannerColor;

			if (color == null) continue;

			Mappings.TryAdd(color.Id, color);
		}
	}

	public static BannerColor FromId(int id)
	{
		if (Mappings.TryGetValue(id, out var color))
			return color;

		throw new Exception("The specified color id is not registered!");
	}
}