using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Alex.API.Utils;
using MiNET.Utils;

namespace Alex.API.Data.Chat
{
	public class TextComponent : BaseComponent
	{
		private static Regex _urlRegex = new Regex("^(?:(https?)://)?([-\\w_\\.]{2,}\\.[a-z]{2,4})(/\\S*)?$", RegexOptions.Compiled);
		/**
	* Converts the old formatting system that used
	* {@link net.md_5.bungee.api.ChatColor#COLOR_CHAR} into the new json based
	* system.
	*
	* @param message the text to convert
	* @return the components needed to print the message to the client
	*/
		public static BaseComponent[] FromLegacyText(String message)
		{
			return FromLegacyText(message, TextColor.White);
		}

		/**
		 * Converts the old formatting system that used
		 * {@link net.md_5.bungee.api.ChatColor#COLOR_CHAR} into the new json based
		 * system.
		 *
		 * @param message the text to convert
		 * @param defaultColor color to use when no formatting is to be applied
		 * (i.e. after ChatColor.RESET).
		 * @return the components needed to print the message to the client
		 */
		public static BaseComponent[] FromLegacyText(String message, TextColor defaultColor)
		{
			List<BaseComponent> components = new List<BaseComponent>();
			StringBuilder builder = new StringBuilder();
			TextComponent component = new TextComponent();
			MatchCollection matcher = _urlRegex.Matches(message);

			TextComponent old;
			for (int i = 0; i < message.Length; i++)
			{
				char c = message[i];
				if (c == '§')
				{
					if (++i >= message.Length)
					{
						break;
					}

					c = message[i];

					if (c >= 'A' && c <= 'Z')
					{
						c += (char)32;
					}
					
					TextColor format = TextColor.GetColor(c);
					if (format == null)
					{
						continue;
					}
					if (builder.Length > 0)
					{
						old = component;
						component = new TextComponent(old);
						old.Text = builder.ToString();
						//old.setText(builder.toString());
						builder = new StringBuilder();
						components.Add(old);
					}
					switch (format.ToString())
					{
						case ChatFormatting.Bold:
							component.Bold = true;
							break;
						case ChatFormatting.Italic:
							component.Italic = true;
							break;
						case ChatFormatting.Underline:
							component.Underlined = true;
							break;
						case ChatFormatting.Strikethrough:
							component.Strikethrough = true;
							break;
						case ChatFormatting.Obfuscated:
							component.Obfuscated = true;
							break;
						default:
							format = defaultColor;
							component = new TextComponent(format.ToString());
							break;
					}
					continue;
				}
				int pos = message.IndexOf(' ', i);
				if (pos == -1)
				{
					pos = message.Length;
				}
				if (_urlRegex.Match(message, i, pos).Success)
				{ //Web link handling

					if (builder.Length > 0)
					{
						old = component;
						component = new TextComponent(old);
						old.Text = builder.ToString();
						builder = new StringBuilder();
						components.Add(old);
					}

					old = component;
					component = new TextComponent(old);
					string urlString = message.Substring(i, pos);
					component.Text = urlString;
					component.ClickEvent = new ClickEvent(ClickEvent.Action.OpenUrl,
						urlString.StartsWith("http") ? urlString : "http://" + urlString);
					
					components.Add(component);
					i += pos - i - 1;
					component = old;
					continue;
				}
				builder.Append(c);
			}

			component.Text = builder.ToString();// .setText(builder.toString());
			components.Add(component);

			return components.ToArray();
		}

		/**
		 * The text of the component that will be displayed to the client
		 */
		public string Text;

		/**
		 * Creates a TextComponent with blank text.
		 */
		public TextComponent()
		{
			this.Text = "";
		}

		/**
		 * Creates a TextComponent with formatting and text from the passed
		 * component
		 *
		 * @param textComponent the component to copy from
		 */
		public TextComponent(TextComponent textComponent) : base(textComponent)
		{
			//copyFormatting(textComponent, FormatRetention.ALL, true);
			//text = textComponent.text;
		}

		/**
		 * Creates a TextComponent with blank text and the extras set to the passed
		 * array
		 *
		 * @param extras the extras to set
		 */
		public TextComponent(params BaseComponent[] extras)
		{
			Text = "";
			SetExtra(new List<BaseComponent>(extras));
		}

		public TextComponent(string textComponent)
		{
			FromLegacyText(textComponent);
		}

		/**
		 * Creates a duplicate of this TextComponent.
		 *
		 * @return the duplicate of this TextComponent.
		 */

		public override BaseComponent Duplicate()
		{
			return new TextComponent(this);
		}


		protected override void ToPlainText(StringBuilder builder)
		{
			builder.Append(Text);
			base.ToPlainText(builder);
		}


		protected override void ToLegacyText(StringBuilder builder)
		{
			builder.Append(GetColor());
			if (IsBold())
			{
				builder.Append(ChatFormatting.Bold);
			}

			if (IsItalic())
			{
				builder.Append(ChatFormatting.Italic);
			}

			if (IsUnderlined())
			{
				builder.Append(ChatFormatting.Underline);
			}

			if (IsStrikethrough())
			{
				builder.Append(ChatFormatting.Strikethrough);
			}

			if (IsObfuscated())
			{
				builder.Append(ChatFormatting.Obfuscated);
			}

			builder.Append(Text);
			base.ToLegacyText(builder);
		}


		public override string ToString()
		{
			return $"TextComponent{{text={Text}, {base.ToString()}}}";
		}
	}
}
