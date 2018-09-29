using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public class PropertyInt : StateProperty<int>
	{
		private readonly int _defaultValue = 0;
		public PropertyInt(string name, int defaultValue = 0) : base(name)
		{
			_defaultValue = defaultValue;
			DefaultValue = defaultValue;
		}

		public override int ParseValue(string value)
		{
			if (int.TryParse(value, out var result))
			{
				return result;
			}

			return _defaultValue;
		}

		public override string ToString(int v)
		{
			return v.ToString().ToLowerInvariant();
		}

		public override object[] GetValidValues()
		{
			throw new System.NotImplementedException();
		}
	}
}
