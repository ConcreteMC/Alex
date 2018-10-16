using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Utils;

namespace Alex.API.Data.Chat
{
	public abstract class BaseComponent
	{
		public BaseComponent Parent;
		/**
		* The color of this component and any child components (unless overridden)
		*/
		public TextColor Color;
		/**
		 * Whether this component and any child components (unless overridden) is
		 * bold
		 */
		public bool Bold;
		/**
		 * Whether this component and any child components (unless overridden) is
		 * italic
		 */
		public bool Italic;
		/**
		 * Whether this component and any child components (unless overridden) is
		 * underlined
		 */
		public bool Underlined;
		/**
		 * Whether this component and any child components (unless overridden) is
		 * strikethrough
		 */
		public bool Strikethrough;
		/**
		 * Whether this component and any child components (unless overridden) is
		 * obfuscated
		 */
		public bool Obfuscated;
		/**
		 * The text to insert into the chat when this component (and child
		 * components) are clicked while pressing the shift key
		 */
		public string Insertion;

		/**
		 * Appended components that inherit this component's formatting and events
		 */
		public List<BaseComponent> Extra;

		/**
		 * The action to perform when this component (and child components) are
		 * clicked
		 */
		public ClickEvent ClickEvent;
		/**
		 * The action to perform when this component (and child components) are
		 * hovered over
		 */
		public HoverEvent HoverEvent;

		public BaseComponent(BaseComponent old)
		{
			CopyFormatting(old, FormatRetention.All, true);

			if (old.Extra != null)
			{
				foreach (BaseComponent extra in old.Extra)
				{
					AddExtra(extra.Duplicate());
				}
			}
		}

		public BaseComponent() { }

		/**
		 * Copies the events and formatting of a BaseComponent. Already set
		 * formatting will be replaced.
		 *
		 * @param component the component to copy from
		 */
		public void CopyFormatting(BaseComponent component)
		{
			CopyFormatting(component, FormatRetention.All, true);
		}

		/**
		 * Copies the events and formatting of a BaseComponent.
		 *
		 * @param component the component to copy from
		 * @param replace if already set formatting should be replaced by the new
		 * component
		 */
		public void CopyFormatting(BaseComponent component, bool replace)
		{
			CopyFormatting(component, FormatRetention.All, replace);
		}

		/**
		 * Copies the specified formatting of a BaseComponent.
		 *
		 * @param component the component to copy from
		 * @param retention the formatting to copy
		 * @param replace if already set formatting should be replaced by the new
		 * component
		 */
		public void CopyFormatting(BaseComponent component, FormatRetention retention, bool replace)
		{
			if (retention == FormatRetention.Events || retention == FormatRetention.All)
			{
				if (replace || ClickEvent == null)
				{
					ClickEvent = component.ClickEvent;
				}
				if (replace || HoverEvent == null)
				{
					HoverEvent = component.HoverEvent;
				}
			}
			if (retention == FormatRetention.Formatting || retention == FormatRetention.All)
			{
				if (replace || Color == null)
				{
					Color = component.GetColorRaw();
				}
				if (replace || Bold == null)
				{
					Bold = component.IsBoldRaw();
				}
				if (replace || Italic == null)
				{
					Italic = component.IsItalicRaw();
				}
				if (replace || Underlined == null)
				{
					Underlined = component.IsUnderlinedRaw();
				}
				if (replace || Strikethrough == null)
				{
					Strikethrough = component.IsStrikethroughRaw();
				}
				if (replace || Obfuscated == null)
				{
					Obfuscated = (component.IsObfuscatedRaw());
				}
				if (replace || Insertion == null)
				{
				 	Insertion = (component.Insertion);
				}
			}
		}

		/**
		 * Retains only the specified formatting.
		 *
		 * @param retention the formatting to retain
		 */
		public void Retain(FormatRetention retention)
		{
			if (retention == FormatRetention.Formatting || retention == FormatRetention.None)
			{
				ClickEvent = null;
				HoverEvent = null;
			}
			if (retention == FormatRetention.Events || retention == FormatRetention.None)
			{
				Color = TextColor.Black;
				Bold = false;
				Italic = false;
				Underlined = false;
				Strikethrough = false;
				Obfuscated = false;
				Insertion = string.Empty;
			}
		}

		/**
		 * Clones the BaseComponent and returns the clone.
		 *
		 * @return The duplicate of this BaseComponent
		 */
		public abstract BaseComponent Duplicate();

		/**
		 * Clones the BaseComponent without formatting and returns the clone.
		 *
		 * @return The duplicate of this BaseComponent
		 * @deprecated API use discouraged, use traditional duplicate
		 */

		public BaseComponent DuplicateWithoutFormatting()
		{
			BaseComponent component = Duplicate();
			component.Retain(FormatRetention.None);
			return component;
		}

		/**
		 * Converts the components to a string that uses the old formatting codes
		 * ({@link net.md_5.bungee.api.ChatColor#COLOR_CHAR}
		 *
		 * @param components the components to convert
		 * @return the string in the old format
		 */
		public static String ToLegacyText(params BaseComponent[] components)
		{
			StringBuilder builder = new StringBuilder();
			foreach (BaseComponent msg in components)
			{
				builder.Append(msg.ToLegacyText());
			}
			return builder.ToString();
		}

		/**
		 * Converts the components into a string without any formatting
		 *
		 * @param components the components to convert
		 * @return the string as plain text
		 */
		public static String ToPlainText(params BaseComponent[] components)
		{
			StringBuilder builder = new StringBuilder();
			foreach (BaseComponent msg in components)
			{
				builder.Append(msg.ToPlainText());
			}
			return builder.ToString();
		}

		/**
		 * Returns the color of this component. This uses the parent's color if this
		 * component doesn't have one. {@link net.md_5.bungee.api.ChatColor#WHITE}
		 * is returned if no color is found.
		 *
		 * @return the color of this component
		 */
		public TextColor GetColor()
		{
			if (Color == null)
			{
				if (Parent == null)
				{
					return TextColor.White;
				}
				return Parent.GetColor();
			}
			return Color;
		}

		/**
		 * Returns the color of this component without checking the parents color.
		 * May return null
		 *
		 * @return the color of this component
		 */
		public TextColor GetColorRaw()
		{
			return Color;
		}

		/**
		 * Returns whether this component is bold. This uses the parent's setting if
		 * this component hasn't been set. false is returned if none of the parent
		 * chain has been set.
		 *
		 * @return whether the component is bold
		 */
		public bool IsBold()
		{
			if (Bold == null)
			{
				return Parent != null && Parent.IsBold();
			}
			return Bold;
		}

		/**
		 * Returns whether this component is bold without checking the parents
		 * setting. May return null
		 *
		 * @return whether the component is bold
		 */
		public bool IsBoldRaw()
		{
			return Bold;
		}

		/**
		 * Returns whether this component is italic. This uses the parent's setting
		 * if this component hasn't been set. false is returned if none of the
		 * parent chain has been set.
		 *
		 * @return whether the component is italic
		 */
		public bool IsItalic()
		{
			if (Italic == null)
			{
				return Parent != null && Parent.IsItalic();
			}
			return Italic;
		}

		/**
		 * Returns whether this component is italic without checking the parents
		 * setting. May return null
		 *
		 * @return whether the component is italic
		 */
		public bool IsItalicRaw()
		{
			return Italic;
		}

		/**
		 * Returns whether this component is underlined. This uses the parent's
		 * setting if this component hasn't been set. false is returned if none of
		 * the parent chain has been set.
		 *
		 * @return whether the component is underlined
		 */
		public bool IsUnderlined()
		{
			if (Underlined == null)
			{
				return Parent != null && Parent.IsUnderlined();
			}
			return Underlined;
		}

		/**
		 * Returns whether this component is underlined without checking the parents
		 * setting. May return null
		 *
		 * @return whether the component is underlined
		 */
		public bool IsUnderlinedRaw()
		{
			return Underlined;
		}

		/**
		 * Returns whether this component is strikethrough. This uses the parent's
		 * setting if this component hasn't been set. false is returned if none of
		 * the parent chain has been set.
		 *
		 * @return whether the component is strikethrough
		 */
		public bool IsStrikethrough()
		{
			if (Strikethrough == null)
			{
				return Parent != null && Parent.IsStrikethrough();
			}
			return Strikethrough;
		}

		/**
		 * Returns whether this component is strikethrough without checking the
		 * parents setting. May return null
		 *
		 * @return whether the component is strikethrough
		 */
		public bool IsStrikethroughRaw()
		{
			return Strikethrough;
		}

		/**
		 * Returns whether this component is obfuscated. This uses the parent's
		 * setting if this component hasn't been set. false is returned if none of
		 * the parent chain has been set.
		 *
		 * @return whether the component is obfuscated
		 */
		public bool IsObfuscated()
		{
			if (Obfuscated == null)
			{
				return Parent != null && Parent.IsObfuscated();
			}
			return Obfuscated;
		}

		/**
		 * Returns whether this component is obfuscated without checking the parents
		 * setting. May return null
		 *
		 * @return whether the component is obfuscated
		 */
		public bool IsObfuscatedRaw()
		{
			return Obfuscated;
		}

		public void SetExtra(List<BaseComponent> components)
		{
			foreach (BaseComponent component in components)
			{
				component.Parent = this;
			}
			Extra = components;
		}

		/**
		 * Appends a text element to the component. The text will inherit this
		 * component's formatting
		 *
		 * @param text the text to append
		 */
		public void AddExtra(string text)
		{
			AddExtra(new TextComponent());
		}

		/**
		 * Appends a component to the component. The text will inherit this
		 * component's formatting
		 *
		 * @param component the component to append
		 */
		public void AddExtra(BaseComponent component)
		{
			if (Extra == null)
			{
				Extra = new List<BaseComponent>();
			}
			component.Parent = this;
			Extra.Add(component);
		}

		/**
		 * Returns whether the component has any formatting or events applied to it
		 *
		 * @return Whether any formatting or events are applied
		 */
		public bool HasFormatting()
		{
			return Color != null || Bold != null
					|| Italic != null || Underlined != null
					|| Strikethrough != null || Obfuscated != null
					|| Insertion != null || HoverEvent != null || ClickEvent != null;
		}

		/**
		 * Converts the component into a string without any formatting
		 *
		 * @return the string as plain text
		 */
		public virtual string ToPlainText()
		{
			StringBuilder builder = new StringBuilder();
			ToPlainText(builder);
			return builder.ToString();
		}

		protected virtual void ToPlainText(StringBuilder builder)
		{
			if (Extra != null)
			{
				foreach (BaseComponent e in Extra)
				{
					e.ToPlainText(builder);
				}
			}
		}

		/**
		 * Converts the component to a string that uses the old formatting codes
		 * ({@link net.md_5.bungee.api.ChatColor#COLOR_CHAR}
		 *
		 * @return the string in the old format
		 */
		public virtual string ToLegacyText()
		{
			StringBuilder builder = new StringBuilder();
			ToLegacyText(builder);
			return builder.ToString();
		}

		protected virtual void ToLegacyText(StringBuilder builder)
		{
			if (Extra != null)
			{
				foreach (BaseComponent e in Extra)
				{
					e.ToLegacyText(builder);
				}
			}
		}

		public static ComponentBuilder WithBuilder()
		{
			return new ComponentBuilder("");
		}
	}
}
