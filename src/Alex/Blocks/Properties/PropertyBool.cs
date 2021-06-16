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

		/// <inheritdoc />
		protected override StateProperty<bool> WithValue(bool value)
		{
			return new PropertyBool(Name, TrueString, FalseString) {Value = value};
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
	}
}
