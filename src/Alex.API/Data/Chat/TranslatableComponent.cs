using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Alex.API.Utils;

namespace Alex.API.Data.Chat
{
	public class TranslatableComponent : BaseComponent
	{
		private Regex format = new Regex("%(?:(\\d+)\\$)?([A-Za-z%]|$)", RegexOptions.Compiled);//.compile( "%(?:(\\d+)\\$)?([A-Za-z%]|$)" );

		/**
		 * The key into the Minecraft locale files to use for the translation. The
		 * text depends on the client's locale setting. The console is always en_US
		 */
		private String translate;
		/**
		 * The components to substitute into the translation
		 */
		private List<BaseComponent> with;

		/**
		 * Creates a translatable component from the original to clone it.
		 *
		 * @param original the original for the new translatable component.
		 */
		public TranslatableComponent(TranslatableComponent original) : base(original)
		{
			this.translate = original.translate;

			if (original.with != null)
			{
				List<BaseComponent> temp = new List<BaseComponent>();
				foreach (BaseComponent baseComponent in original.with)
				{
					temp.Add(baseComponent.Duplicate());
				}
				setWith(temp);
			}
		}

		public TranslatableComponent(string translate, params Object[] with)
		{
			this.translate = translate;
			if (with != null && with.Length != 0)
			{
				List<BaseComponent> temp = new List<BaseComponent>();
				foreach (Object w in with)
				{
					if (w is BaseComponent )
					{
						temp.Add((BaseComponent)w);
					} else
					{
						temp.Add(new TextComponent(w.ToString()));
					}
				}
				setWith(temp);
			}
		}

	public void setWith(List<BaseComponent> components)
		{
			foreach (BaseComponent component in components)
			{
				component.Parent = this;
			}
			with = components;
		}

		/**
		 * Adds a text substitution to the component. The text will inherit this
		 * component's formatting
		 *
		 * @param text the text to substitute
		 */
		public void addWith(String text)
		{
			addWith(new TextComponent(text));
		}

		/**
		 * Adds a component substitution to the component. The text will inherit
		 * this component's formatting
		 *
		 * @param component the component to substitute
		 */
		public void addWith(BaseComponent component)
		{
			if (with == null)
			{
				with = new List<BaseComponent>();
			}
			component.Parent = this;
			with.Add(component);
		}

		/*protected override void ToPlainText(StringBuilder builder)
		{
			String trans = translate;// TranslationRegistry.INSTANCE.translate(translate);

			Matcher matcher = format.matcher(trans);
			int position = 0;
			int i = 0;
			while (matcher.find(position))
			{
				int pos = matcher.start();
				if (pos != position)
				{
					builder.append(trans.substring(position, pos));
				}
				position = matcher.end();

				String formatCode = matcher.group(2);
				switch (formatCode.charAt(0))
				{
					case 's':
					case 'd':
						String withIndex = matcher.group(1);
						with.get(withIndex != null ? Integer.parseInt(withIndex) - 1 : i++).toPlainText(builder);
						break;
					case '%':
						builder.append('%');
						break;
				}
			}
			if (trans.length() != position)
			{
				builder.append(trans.substring(position, trans.length()));
			}

			super.toPlainText(builder);
		}

		protected override void ToLegacyText(StringBuilder builder)
		{
			string trans = translate; //TranslationRegistry.INSTANCE.translate(translate);
			
			var matches = format.Matches(trans);
			foreach (var match in matches)
			{

			}

			//Matcher matcher = format.matcher(trans);
			int position = 0;
			int i = 0;
			var a = format.Match(trans, position);
			while (a.Success)
			{
				int pos = a.Index;
				if (pos != position)
				{
					addFormat(builder);
					builder.Append(trans.Substring(position, pos));
				}

				position = a.Index + a.Length;

				String formatCode = matcher.group(2);
				switch (formatCode[0])
				{
					case 's':
					case 'd':
						String withIndex = matcher.group(1);
						with[withIndex != null ? int.Parse(withIndex) - 1 : i++].ToLegacyText();
						break;
					case '%':
						addFormat(builder);
						builder.Append('%');
						break;
				}
			}
			if (trans.Length != position)
			{
				addFormat(builder);
				builder.Append(trans.Substring(position, trans.Length));
			}
		
			base.ToLegacyText(builder);
		}*/

		private void addFormat(StringBuilder builder)
		{
			builder.Append(GetColorRaw());
			if (Bold)
			{
				builder.Append(TextColor.Bold);
			}
			if (Italic)
			{
				builder.Append(TextColor.Italic);
			}
			if (Underlined)
			{
				builder.Append(TextColor.Underline);
			}
			if (Strikethrough)
			{
				builder.Append(TextColor.Strikethrough);
			}
			if (Obfuscated)
			{
				builder.Append(TextColor.Obfuscated);
			}
		}

		public override BaseComponent Duplicate()
		{
			return new TranslatableComponent(this);
		}
	}
}
