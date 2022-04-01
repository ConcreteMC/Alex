using System;
using NLog;

namespace Alex.Networking.Java.Models
{
	public class ChatObject
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChatObject));

		public ChatObject(string message)
		{
			RawMessage = message;
		}

		public string RawMessage { get; set; }

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

		public static bool TryParse(string json, out string rawText)
		{
			try
			{
				string parsedChat = ChatParser.ParseText(json);
				rawText = parsedChat;
				
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not parse chat: {ex}");
				//   result = null;
				rawText = json;

				return false;
			}
		}
	}
}