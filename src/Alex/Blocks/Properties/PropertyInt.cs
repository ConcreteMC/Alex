using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public class PropertyInt : StateProperty<int>
	{
		private readonly int _defaultValue = 0;
		public PropertyInt(string name, int defaultValue = 0) : base(name)
		{
			_defaultValue = defaultValue;
		}

		public override int ParseValue(string value)
		{
			if (int.TryParse(value, out var result))
			{
				return result;
			}

			return _defaultValue;
		}
	}
}
