using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public class PropertyInt : StateProperty<int>
	{
		private int DefaultValue = 0;
		public PropertyInt(string name, int defaultValue = 0) : base(name)
		{
			DefaultValue = defaultValue;
		}

		public override int ParseValue(string value)
		{
			if (int.TryParse(value, out var result))
			{
				return result;
			}

			return DefaultValue;
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
