using System.Collections.Generic;
using System.Linq;
using Alex.API.Utils;

namespace Alex.API.Data.Chat
{
	public class ComponentBuilder : IJoiner
	{
		private BaseComponent _current;
		private List<BaseComponent> _parts = new List<BaseComponent>();

		/**
		 * Creates a ComponentBuilder from the other given ComponentBuilder to clone
		 * it.
		 *
		 * @param original the original for the new ComponentBuilder.
		 */
		public ComponentBuilder(ComponentBuilder original)
		{
			_current = original._current.Duplicate();
			foreach (BaseComponent baseComponent in original._parts)
			{
				_parts.Add(baseComponent.Duplicate());
			}
		}

		/**
		 * Creates a ComponentBuilder with the given text as the first part.
		 *
		 * @param text the first text element
		 */
		public ComponentBuilder(string text)
		{
			_current = new TextComponent(text);
		}

		/**
		 * Creates a ComponentBuilder with the given component as the first part.
		 *
		 * @param component the first component element
		 */
		public ComponentBuilder(BaseComponent component)
		{
			_current = component.Duplicate();
		}

		/**
		 * Appends a component to the builder and makes it the current target for
		 * formatting. The component will have all the formatting from previous
		 * part.
		 *
		 * @param component the component to append
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Append(BaseComponent component)
		{
			return Append(component, FormatRetention.All);
		}

		/**
		 * Appends a component to the builder and makes it the current target for
		 * formatting. You can specify the amount of formatting retained from
		 * previous part.
		 *
		 * @param component the component to append
		 * @param retention the formatting to retain
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Append(BaseComponent component, FormatRetention retention)
		{
			_parts.Add(_current);

			BaseComponent previous = _current;
			_current = component.Duplicate();
			_current.CopyFormatting(previous, retention, false);
			return this;
		}

		/**
		 * Appends the components to the builder and makes the last element the
		 * current target for formatting. The components will have all the
		 * formatting from previous part.
		 *
		 * @param components the components to append
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Append(BaseComponent[] components)
		{
			return Append(components, FormatRetention.All);
		}

		/**
		 * Appends the components to the builder and makes the last element the
		 * current target for formatting. You can specify the amount of formatting
		 * retained from previous part.
		 *
		 * @param components the components to append
		 * @param retention the formatting to retain
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Append(BaseComponent[] components, FormatRetention retention)
		{
			if (components.Length == 0) return this;
			//Preconditions.checkArgument(components.Length != 0, "No components to append");

			BaseComponent previous = _current;
			foreach (BaseComponent component in components)
			{
				_parts.Add(_current);

				_current = component.Duplicate();
				_current.CopyFormatting(previous, retention, false);
			}

			return this;
		}

		/**
		 * Appends the text to the builder and makes it the current target for
		 * formatting. The text will have all the formatting from previous part.
		 *
		 * @param text the text to append
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Append(string text)
		{
			return Append(text, FormatRetention.All);
		}

		/**
		 * Appends the text to the builder and makes it the current target for
		 * formatting. You can specify the amount of formatting retained from
		 * previous part.
		 *
		 * @param text the text to append
		 * @param retention the formatting to retain
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Append(string text, FormatRetention retention)
		{
			_parts.Add(_current);

			BaseComponent old = _current;
			_current = new TextComponent(text);
			_current.CopyFormatting(old, retention, false);

			return this;
		}

		/**
		 * Allows joining additional components to this builder using the given
		 * {@link Joiner} and {@link FormatRetention#ALL}.
		 *
		 * Simply executes the provided joiner on this instance to facilitate a
		 * chain pattern.
		 *
		 * @param joiner joiner used for operation
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Join(IJoiner joiner)
		{
			return joiner.Join(this, FormatRetention.All);
		}

		/**
		 * Allows joining additional components to this builder using the given
		 * {@link Joiner}.
		 *
		 * Simply executes the provided joiner on this instance to facilitate a
		 * chain pattern.
		 *
		 * @param joiner joiner used for operation
		 * @param retention the formatting to retain
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Join(IJoiner joiner, FormatRetention retention)
		{
			return joiner.Join(this, retention);
		}

		/**
		 * Sets the color of the current part.
		 *
		 * @param color the new color
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Color(TextColor color)
		{
			_current.Color = color;
			return this;
		}

		/**
		 * Sets whether the current part is bold.
		 *
		 * @param bold whether this part is bold
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Bold(bool bold)
		{
			_current.Bold = bold;
			return this;
		}

		/**
		 * Sets whether the current part is italic.
		 *
		 * @param italic whether this part is italic
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Italic(bool italic)
		{
			_current.Italic = italic;
			return this;
		}

		/**
		 * Sets whether the current part is underlined.
		 *
		 * @param underlined whether this part is underlined
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Underlined(bool underlined)
		{
			_current.Underlined = underlined;
			return this;
		}

		/**
		 * Sets whether the current part is strikethrough.
		 *
		 * @param strikethrough whether this part is strikethrough
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Strikethrough(bool strikethrough)
		{
			_current.Strikethrough = strikethrough;
			return this;
		}

		/**
		 * Sets whether the current part is obfuscated.
		 *
		 * @param obfuscated whether this part is obfuscated
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Obfuscated(bool obfuscated)
		{
			_current.Obfuscated = obfuscated;
			return this;
		}

		/**
		 * Sets the insertion text for the current part.
		 *
		 * @param insertion the insertion text
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder Insertion(string insertion)
		{
			_current.Insertion = insertion;
			return this;
		}

		/**
		 * Sets the click event for the current part.
		 *
		 * @param clickEvent the click event
		 * @return this ComponentBuilder for chaining
		 */
		public ComponentBuilder ClickEvent(ClickEvent clickEvent)

		{
			_current.ClickEvent = clickEvent;
			return this;
		}

		/**
	     * Sets the hover event for the current part.
	     *
	     * @param hoverEvent the hover event
	     * @return this ComponentBuilder for chaining
	     */
		public ComponentBuilder HoverEvent(HoverEvent hoverEvent)

		{
			_current.HoverEvent = hoverEvent;
			return this;
		}

		/**
	     * Sets the current part back to normal settings. Only text is kept.
	     *
	     * @return this ComponentBuilder for chaining
	     */
		public ComponentBuilder Reset()
		{
			return Retain(FormatRetention.None);
		}

		/**
	     * Retains only the specified formatting. Text is not modified.
	     *
	     * @param retention the formatting to retain
	     * @return this ComponentBuilder for chaining
	     */
		public ComponentBuilder Retain(FormatRetention retention)
		{
			_current.Retain(retention);
			return this;
		}

		/**
	     * Returns the components needed to display the message created by this
	     * builder.
	     *
	     * @return the created components
	     */
		public BaseComponent[] Create()
		{
			return _parts.Append(_current).ToArray();
		}
	}

	public enum FormatRetention
	{

		/**
		 * Specify that we do not want to retain anything from the previous
		 * component.
		 */
		None,

		/**
		 * Specify that we want the formatting retained from the previous
		 * component.
		 */
		Formatting,

		/**
		 * Specify that we want the events retained from the previous component.
		 */
		Events,

		/**
		 * Specify that we want to retain everything from the previous
		 * component.
		 */
		All
	}

	/**
	 * Functional interface to join additional components to a ComponentBuilder.
	 */
	public interface IJoiner
	{

		/**
		 * Joins additional components to the provided {@link ComponentBuilder}
		 * and then returns it to fulfill a chain pattern.
		 *
		 * Retention may be ignored and is to be understood as an optional
		 * recommendation to the Joiner and not as a guarantee to have a
		 * previous component in builder unmodified.
		 *
		 * @param componentBuilder to which to append additional components
		 * @param retention the formatting to possibly retain
		 * @return input componentBuilder for chaining
		 */
		ComponentBuilder Join(IJoiner componentBuilder, FormatRetention retention);
	}
}
