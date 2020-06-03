using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.API.Utils
{
    public class ChatObjectComponent
    {
        public string text { get; set; } = null;
        //	public string clickEvent { get; set; } = null;
        //	public string hoverEvent { get; set; } = null;

        public string insertion { get; set; } = null;
        public bool bold { get; set; } = false;
        public bool italic { get; set; } = false;
        public bool underlined { get; set; } = false;
        public bool strikethrough { get; set; } = false;
        public bool obfuscated { get; set; } = false;
        public string color { get; set; } = null;
    }

    public class TextColor
    {
        private static Logger Log = LogManager.GetCurrentClassLogger(typeof(TextColor));

        public const char Prefix = '§';
        
        // @formatter:off — disable formatter after this line
        public static readonly TextColor Black       = new TextColor('0',   0,   0,   0,   0,   0,   0, "black");
        public static readonly TextColor DarkBlue    = new TextColor('1',   0,   0, 170,   0,   0,  42, "dark_blue");
        public static readonly TextColor DarkGreen   = new TextColor('2',   0, 170,   0,  42,  42,   0, "dark_green");
        public static readonly TextColor DarkCyan    = new TextColor('3',   0, 170, 170,  42,  42,  42, "dark_aqua");
        public static readonly TextColor DarkRed     = new TextColor('4', 170,   0,   0,   0,   0,   0, "dark_red");
        public static readonly TextColor Purple      = new TextColor('5', 170,   0, 170,   0,   0,  42, "dark_purple");
        public static readonly TextColor Gold        = new TextColor('6', 255, 170,   0,  42,  42,   0, "gold");
        public static readonly TextColor Gray        = new TextColor('7', 170, 170, 170,  42,  42,  42, "gray");
        public static readonly TextColor DarkGray    = new TextColor('8',  85,  85,  85,  21,  21,  21, "dark_gray");
        public static readonly TextColor Blue        = new TextColor('9',  85,  85, 255,  21,  21,  63, "blue");
        public static readonly TextColor BrightGreen = new TextColor('a',  85, 255, 85,  21,  63,  21, "green");
        public static readonly TextColor Cyan        = new TextColor('b',  85, 255, 255,  21,  63,  63, "aqua");
        public static readonly TextColor Red         = new TextColor('c', 255,  85,  85,  63,  21,  21, "red");
        public static readonly TextColor Pink        = new TextColor('d', 255,  85, 255,  63,  21,  62, "light_purple");
        public static readonly TextColor Yellow      = new TextColor('e', 255, 255,  85,  63,  63,  21, "yellow");
        public static readonly TextColor White       = new TextColor('f', 255, 255, 255,  63,  63,  63, "white");

		public static readonly TextColor Obfuscated = new TextColor('k', "magic");
		public static readonly TextColor Bold = new TextColor('l', "bold");
		public static readonly TextColor Strikethrough = new TextColor('m', "strikethrough");
		public static readonly TextColor Underline = new TextColor('n', "underline");
		public static readonly TextColor Italic = new TextColor('o', "italic");
		public static readonly TextColor Reset = new TextColor('r', "reset");
		// @formatter:on — enable formatter after this line

		public string Name;
        public Color  ForegroundColor;
        public Color  BackgroundColor;
        public char   Code;

	    public TextColor(char code, string name)
	    {
		    Name = name;
		    Code = code;
	    }

        public TextColor(char code, int r, int g, int b, int br, int bg, int bb, string name)
        {
            try
            {
                this.Name = name;
                this.Code = code;
                this.ForegroundColor = new Color(r, g, b);
                this.BackgroundColor = new Color(br, bg, bb);
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception: " + ex.ToString());
            }
        }

	    public TextColor(Color c)
	    {
			ForegroundColor = c;
            BackgroundColor = Color.TransparentBlack;
	    }

	    public override string ToString()
	    {
		    return $"§{Code}";
	    }

	    public static string Color2tag(string colorname)
        {
            switch (colorname.ToLower())
            {
                case "black": /*  Blank if same  */
                    return "§0";
                case "dark_blue":
                    return "§1";
                case "dark_green":
                    return "§2";
                case "dark_aqua":
                case "dark_cyan":
                    return "§3";
                case "dark_red":
                    return "§4";
                case "dark_purple":
                case "dark_magenta":
                    return "§5";
                case "gold":
                case "dark_yellow":
                    return "§6";
                case "gray":
                    return "§7";
                case "dark_gray":
                    return "§8";
                case "blue":
                    return "§9";
                case "green":
                    return "§a";
                case "aqua":
                case "cyan":
                    return "§b";
                case "red":
                    return "§c";
                case "light_purple":
                case "magenta":
                    return "§d";
                case "yellow":
                    return "§e";
                case "white":
                    return "§f";

	            case "obfuscated":
		            return Obfuscated.ToString();
				case "bold":
		            return Bold.ToString();
	            case "strikethrough":
		            return Strikethrough.ToString();
	            case "underline":
		            return Underline.ToString();
	            case "italic":
		            return Italic.ToString();
	            case "reset":
		            return Reset.ToString();

				default:
                    return "";
            }
        }

        public static TextColor GetColor(char col)
        {
            switch (col)
            {
                case '0':
                    return Black;
                case '1':
                    return DarkBlue;
                case '2':
                    return DarkGreen;
                case '3':
                    return DarkCyan;
                case '4':
                    return DarkRed;
                case '5':
                    return Purple;
                case '6':
                    return Gold;
                case '7':
                    return Gray;
                case '8':
                    return DarkGray;
                case '9':
                    return Blue;
                case 'a':
                    return BrightGreen;
                case 'b':
                    return Cyan;
				case 'c':
					return Red;
                case 'd':
                    return Pink;
                case 'e':
                    return Yellow;
                case 'f':
                    return White;

				case 'k':
					return Obfuscated;
				case 'l':
					return Bold;
				case 'm':
					return Strikethrough;
				case 'n':
					return Underline;
				case 'o':
					return Italic;
				case 'r':
					return Reset;

                default:
                    return White;
            }
        }

        public static TextColor GetColor(string colorname)
        {
            switch (colorname.ToLower())
            {
                case "black":
                    return Black;
                case "dark_blue":
                    return DarkBlue;
                case "dark_green":
                    return DarkGreen;
                case "dark_aqua":
                case "dark_cyan":
                    return DarkCyan;
                case "dark_red":
                    return DarkRed;
                case "dark_purple":
                case "dark_magenta":
                    return Purple;
                case "gold":
                case "dark_yellow":
                    return Gold;
                case "gray":
                    return Gray;
                case "dark_gray":
                    return DarkGray;
                case "blue":
                    return Blue;
                case "green":
                    return BrightGreen;
                case "aqua":
                case "cyan":
                    return Cyan;
                case "red":
                    return Red;
                case "light_purple":
                case "magenta":
                    return Pink;
                case "yellow":
                    return Yellow;
                case "white":
                    return White;

	            case "obfuscated":
		            return Obfuscated;
	            case "bold":
		            return Bold;
	            case "strikethrough":
		            return Strikethrough;
	            case "underline":
		            return Underline;
	            case "italic":
		            return Italic;
	            case "reset":
		            return Reset;

				default: return White;
            }
        }

        private static readonly TextColor[] RainbowColors = new TextColor[]
        {
            DarkRed,
            Red,
            Gold,
            Yellow,
            BrightGreen,
            DarkGreen,
            Cyan,
            Blue,
            DarkBlue,
            Purple,
            Pink
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
        
        public static explicit operator Color(TextColor textColor)
        {
            return textColor.ForegroundColor;
        }

        public static explicit operator TextColor(Color color)
        {
            return new TextColor(color);
        }
    }

    public class ChatObject
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChatObject));

        public ChatObject(string message)
        {
            RawMessage = message;
        }

        public string RawMessage { get; set; }

        private static Random _rnd = new Random();

        /*
        public Vector2 Render(SpriteBatch sb, SpriteFont font, Vector2 position)
        {
            Vector2 pos = position;

            TextColor color         = TextColor.White;
            bool      bold          = false;
            bool      italic        = false;
            bool      strikethrough = false;
            bool      underlined    = false;
            bool      obfuscated    = false;

            string msg = RawMessage;
            if (string.IsNullOrWhiteSpace(msg))
            {
                Log.Info($"Message to output is null!");
                return pos;
            }

            for (var index = 0; index < msg.Length; index++)
            {
                var character = msg[index];

                if (character == '§')
                {
                    if (index + 1 <= msg.Length - 1)
                    {
                        var c = msg[index + 1];
                        if (c == 'r')
                        {
                            color         = TextColor.White;
                            bold          = false;
                            italic        = false;
                            strikethrough = false;
                            underlined    = false;
                            obfuscated    = false;
                        }
                        else if (c == 'l')
                        {
                            bold = true;
                        }
                        else if (c == 'm')
                        {
                            strikethrough = true;
                        }
                        else if (c == 'n')
                        {
                            underlined = true;
                        }
                        else if (c == 'o')
                        {
                            italic = true;
                        }
                        else if (c == 'k')
                        {
                            obfuscated = true;
                        }
                        else
                        {
                            color = TextColor.GetColor(c);
                        }

                        index += 1;
                        continue;
                    }
                }

                string cs   = character.ToString();
                var    size = font.MeasureString(cs);
                if (obfuscated)
                {
                    var options = font.GetGlyphs().Where(x => x.Value.Character != character && x.Value.Width == size.X)
                                      .ToArray();
                    var result = options[_rnd.Next(0, options.Length - 1)].Value;
                    size = new Vector2(result.Width, result.BoundsInTexture.Height);
                    cs   = result.Character.ToString();
                }

                sb.DrawString(font, cs, pos + new Vector2(size.X * 0.125f, size.Y * 0.125f),
                              color.BackgroundColor);
                sb.DrawString(font, cs, pos, color.ForegroundColor);

                pos.X += size.X;
            }


            return position;
        }
		*/

        public static bool TryParse(string json, out ChatObject result)
        {
            try
            {
                List<string> links      = new List<string>();
                string       parsedChat = ChatParser.ParseText(json, links);
                result = new ChatObject(parsedChat);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Could not parse chat: {ex}");
                result = null;
                return false;
            }
        }
    }
}