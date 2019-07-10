using System.Text;
using Newtonsoft.Json;

namespace Alex.API.Data.Chat
{
	public class ScoreComponent : BaseComponent
	{

		/**
	     * The name of the entity whose score should be displayed.
	     */
		[JsonProperty("name")]
		public string Name;

		/**
	     * The internal name of the objective the score is attached to.
	     */
		[JsonProperty("objective")]
		public string Objective;

		/**
	     * The optional value to use instead of the one present in the Scoreboard.
	     */
		[JsonProperty("value")]
		public string Value = "";

		/**
	     * Creates a new score component with the specified name and objective.<br>
	     * If not specifically set, value will default to an empty string;
	     * signifying that the scoreboard value should take precedence. If not null,
	     * nor empty, {@code value} will override any value found in the
	     * scoreboard.<br>
	     * The value defaults to an empty string.
	     *
	     * @param name the name of the entity, or an entity selector, whose score
	     * should be displayed
	     * @param objective the internal name of the objective the entity's score is
	     * attached to
	     */
		public ScoreComponent(string name, string objective)
		{
			Name = name;
			Objective = objective;
		}

		/**
	     * Creates a score component from the original to clone it.
	     *
	     * @param original the original for the new score component
	     */
		public ScoreComponent(ScoreComponent original) : base(original)
		{
			Name = (original.Name);
			Objective = original.Objective;
			Value = (original.Value);
		}

		public override BaseComponent Duplicate()
		{
			return new ScoreComponent(this);
		}

		protected override void ToLegacyText(StringBuilder builder)
		{
			builder.Append(this.Value);
			base.ToLegacyText(builder);
		}
	}
}
