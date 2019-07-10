using System.Text;

namespace Alex.API.Data.Chat
{
	public class SelectorComponent : BaseComponent
	{

		/**
	     * An entity target selector (@p, @a, @r, @e, or @s) and, optionally,
	     * selector arguments (e.g. @e[r=10,type=Creeper]).
	     */
		public string Selector;

		/**
	     * Creates a selector component from the original to clone it.
	     *
	     * @param original the original for the new selector component
	     */
		public SelectorComponent(SelectorComponent original)
		{
			Selector = original.Selector;
		}

		public override BaseComponent Duplicate()
		{
			return new SelectorComponent(this);
		}

		protected override void ToLegacyText(StringBuilder builder)
		{
			builder.Append(this.Selector);
			base.ToLegacyText(builder);
		}
	}
}
