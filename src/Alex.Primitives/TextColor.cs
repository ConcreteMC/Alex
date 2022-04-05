using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Alex.Interfaces
{
	public class TextColor
	{
		private static Logger Log = LogManager.GetCurrentClassLogger(typeof(TextColor));

		public const char Prefix = '§';

		// @formatter:off — disable formatter after this line
		// BG = Math.Floor(FG/4f);
		public static readonly TextColor Black       = new TextColor('0',   0,   0,   0,   0,   0,   0, "black");
		public static readonly TextColor DarkBlue    = new TextColor('1',   0,   0, 170,   0,   0,  42, "dark_blue");
		public static readonly TextColor DarkGreen   = new TextColor('2',   0, 170,   0,  42,  42,   0, "dark_green");
		public static readonly TextColor DarkCyan    = new TextColor('3',   0, 170, 170,  42,  42,  42, "dark_aqua") { AltNames = new []{"dark_cyan"}};
		public static readonly TextColor DarkRed     = new TextColor('4', 170,   0,   0,   0,   0,   0, "dark_red");
		public static readonly TextColor Purple      = new TextColor('5', 170,   0, 170,   0,   0,  42, "dark_purple") { AltNames = new []{"dark_magenta"}};
		public static readonly TextColor Gold        = new TextColor('6', 255, 170,   0,  42,  42,   0, "gold") { AltNames = new []{"dark_yellow"}};
		public static readonly TextColor Gray        = new TextColor('7', 170, 170, 170,  42,  42,  42, "gray");
		public static readonly TextColor DarkGray    = new TextColor('8',  85,  85,  85,  21,  21,  21, "dark_gray");
		public static readonly TextColor Blue        = new TextColor('9',  85,  85, 255,  21,  21,  63, "blue");
		public static readonly TextColor BrightGreen = new TextColor('a',  85, 255, 85,  21,  63,  21, "green");
		public static readonly TextColor Cyan        = new TextColor('b',  85, 255, 255,  21,  63,  63, "aqua") { AltNames = new []{"cyan"}};
		public static readonly TextColor Red         = new TextColor('c', 255,  85,  85,  63,  21,  21, "red");
		public static readonly TextColor Pink        = new TextColor('d', 255,  85, 255,  63,  21,  62, "light_purple") { AltNames = new []{"magenta"}};
		public static readonly TextColor Yellow      = new TextColor('e', 255, 255,  85,  63,  63,  21, "yellow");
		public static readonly TextColor White       = new TextColor('f', 255, 255, 255,  63,  63,  63, "white");

		public static readonly TextColor Obfuscated = new TextColor('k', "magic");
		public static readonly TextColor Bold = new TextColor('l', "bold");
		public static readonly TextColor Strikethrough = new TextColor('m', "strikethrough");
		public static readonly TextColor Underline = new TextColor('n', "underline");
		public static readonly TextColor Italic = new TextColor('o', "italic");
		public static readonly TextColor Reset = new TextColor('r', "reset");
		
		
		
		// @formatter:on — enable formatter after this line

		public static readonly TextColor[] Colors = new[]
		{
			Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, Purple, Gold, Gray, DarkGray, Blue, BrightGreen, Cyan,
			Red, Pink, Yellow, White,
		};

		public static readonly TextColor[] Formatters = new[]
		{
			Obfuscated, Bold, Strikethrough, Underline, Italic, Reset
		};

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				UpdateAliases();
			}
		}

		public string[] AltNames
		{
			get => _altNames;
			set
			{
				_altNames = value;
				UpdateAliases();
			}
		}

		public string[] Aliases { get; private set; }

		public IColor ForegroundColor;
		public IColor BackgroundColor;
		public char Code;
		private string[] _altNames;
		private string _name;

		public TextColor(char code, string name, params string[] altNames)
		{
			Name = name;
			Code = code;
			AltNames = altNames;
		}

		public TextColor(char code,
			byte r,
			byte g,
			byte b,
			byte br,
			byte bg,
			byte bb,
			string name,
			params string[] altNames) : this(code, name, altNames)
		{
			try
			{
				ForegroundColor = Primitives.Factory.Color(r, g, b);
				BackgroundColor = Primitives.Factory.Color(br, bg, bb);
			}
			catch (Exception ex)
			{
				Log.Warn($"Exception: " + ex.ToString());
			}
		}

		public TextColor(IColor c, bool lookupColor = true)
		{
			ForegroundColor = c;

			if (lookupColor && TryMatchColorByForegroundColor(c, out TextColor match))
			{
				BackgroundColor = match.BackgroundColor;
				Code = match.Code;
				Name = match.Name;
				AltNames = match.AltNames;
			}
			else
			{
				BackgroundColor = Primitives.Factory.Color(ToBackgroundColor(c.R), ToBackgroundColor(c.G), ToBackgroundColor(c.B));
			}
		}

		private void UpdateAliases()
		{
			if (AltNames == null || AltNames.Length == 0)
			{
				Aliases = new[] { Name };

				return;
			}

			var list = new List<string>();
			list.AddRange(AltNames);
			list.Add(Name);
			Aliases = list.ToArray();
		}

		public static bool TryMatchColorByForegroundColor(IColor color, out TextColor textColor)
		{
			foreach (var allColor in Colors)
			{
				if (allColor.ForegroundColor.Equals(color))
				{
					textColor = allColor;

					return true;
				}
			}

			textColor = default;

			return false;
		}

		public static bool TryMatchColorByName(string name, out TextColor textColor)
		{
			foreach (var allColor in Colors)
			{
				if (allColor.Aliases.Any(alias => string.Equals(name, alias, StringComparison.OrdinalIgnoreCase)))
				{
					textColor = allColor;

					return true;
				}
			}

			foreach (var allColor in Formatters)
			{
				if (allColor.Aliases.Any(alias => string.Equals(name, alias, StringComparison.OrdinalIgnoreCase)))
				{
					textColor = allColor;

					return true;
				}
			}

			textColor = default;

			return false;
		}

		public static bool TryMatchColorByCode(char code, out TextColor textColor)
		{
			switch (code)
			{
				case '0':
					textColor = Black;

					return true;

				case '1':
					textColor = DarkBlue;

					return true;

				case '2':
					textColor = DarkGreen;

					return true;

				case '3':
					textColor = DarkCyan;

					return true;

				case '4':
					textColor = DarkRed;

					return true;

				case '5':
					textColor = Purple;

					return true;

				case '6':
					textColor = Gold;

					return true;

				case '7':
					textColor = Gray;

					return true;

				case '8':
					textColor = DarkGray;

					return true;

				case '9':
					textColor = Blue;

					return true;

				case 'a':
					textColor = BrightGreen;

					return true;

				case 'b':
					textColor = Cyan;

					return true;

				case 'c':
					textColor = Red;

					return true;

				case 'd':
					textColor = Pink;

					return true;

				case 'e':
					textColor = Yellow;

					return true;

				case 'f':
					textColor = White;

					return true;

				case 'k':
					textColor = Obfuscated;

					return true;

				case 'l':
					textColor = Bold;

					return true;

				case 'm':
					textColor = Strikethrough;

					return true;

				case 'n':
					textColor = Underline;

					return true;

				case 'o':
					textColor = Italic;

					return true;

				case 'r':
					textColor = Reset;

					return true;
			}

			textColor = default;

			return false;
		}

		private static byte ToBackgroundColor(int foreground)
		{
			if (foreground <= 0)
				return 0;

			return (byte)Math.Floor(foreground / 4f);
		}

		public override string ToString()
		{
			return $"{Prefix}{Code}";
		}

		public static string Color2tag(string colorname)
		{
			if (TryMatchColorByName(colorname, out var textColor))
			{
				return textColor.ToString();
			}

			return string.Empty;
		}

		public static TextColor GetColor(char col)
		{
			if (TryMatchColorByCode(col, out var textColor))
				return textColor;

			return White;
		}

		public static TextColor GetColor(string colorname)
		{
			if (TryMatchColorByName(colorname, out var textColor))
			{
				return textColor;
			}

			return White;
		}

		private static readonly TextColor[] RainbowColors = new TextColor[]
		{
			DarkRed, Red, Gold, Yellow, BrightGreen, DarkGreen, Cyan, Blue, DarkBlue, Purple, Pink
		};

		public static string Rainbow(string input)
		{
			StringBuilder sb = new StringBuilder();

			for (var index = 0; index < input.Length; index++)
			{
				char c = input[index];

				sb.Append(Prefix);
				sb.Append(RainbowColors[index % RainbowColors.Length].Code);
				sb.Append(c);
			}

			sb.Append(Prefix);
			sb.Append(Reset.Code);

			return sb.ToString();
		}

		public static IColor ToIColor(TextColor textColor) {
			return textColor.ForegroundColor;
		}

		public static TextColor FromIColor(IColor color) {
			if (TryMatchColorByForegroundColor(color, out var textColor))
				return textColor;

			return new TextColor(color, false);
		}
	}
}