using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public class PropertyString : StateProperty<string>
	{
		private readonly string _defaultValue = "";
		public PropertyString(string name, string defaultValue = "") : base(name)
		{
			_defaultValue = defaultValue;
			Value = defaultValue;
		}

		/// <inheritdoc />
		public override StateProperty<string> WithValue(string value)
		{
			return new PropertyString(Name, _defaultValue) {Value = value};
		}

		public override string ParseValue(string value)
		{
			return value;
		}
	}
}