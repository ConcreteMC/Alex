using System;
using Alex.Blocks.State;

namespace Alex.Blocks.Properties
{
	public class PropertyBool : StateProperty<bool>
	{
		public PropertyBool(string name) : this(name, "true", "false")
		{
		
		}

		private string TrueString;
		private string FalseString;
		public PropertyBool(string name, string trueS, string falseS) : base(name)
		{
			TrueString = trueS;
			FalseString = falseS;
		}

		public override bool ParseValue(string value)
		{
			if (bool.TryParse(value, out var result))
			{
				return result;
			}

			if (value.Equals(TrueString, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		public override string ToString(bool v)
		{
			if (!string.IsNullOrWhiteSpace(TrueString) && v)
			{
				return TrueString.ToLowerInvariant();
			}

			if (!string.IsNullOrWhiteSpace(FalseString) && !v)
			{
				return FalseString.ToLowerInvariant();
			}

			return v.ToString().ToLowerInvariant();
		}

		public override object[] GetValidValues()
		{
			return new object[]
			{
				true,
				false,
				TrueString,
				FalseString
			};
		}
	}
}
