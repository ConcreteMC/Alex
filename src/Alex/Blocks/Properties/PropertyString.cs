using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

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
		public override IStateProperty<string> WithValue(string value)
		{
			return new PropertyString(Name, _defaultValue) {Value = value};
		}

		public override string ParseValue(string value)
		{
			return value;
		}
	}
}