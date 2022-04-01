using System;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils;
using Alex.Interfaces;

namespace Alex.Blocks.Properties
{
	public class PropertyWallConnection : StateProperty<bool>
	{
		public PropertyWallConnection(string name) : this(name, "true", "false") { }

		private string TrueString;
		private string FalseString;

		public PropertyWallConnection(string name, string trueS, string falseS) : base(name)
		{
			TrueString = trueS;
			FalseString = falseS;
		}

		/// <inheritdoc />
		public override IStateProperty<bool> WithValue(bool value)
		{
			return new PropertyBool(Name, TrueString, FalseString) { Value = value };
		}

		public override bool ParseValue(string value)
		{
			if (bool.TryParse(value, out var result))
			{
				return result;
			}

			if (string.Equals(value, TrueString, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public override string ToFormattedString()
		{
			return $"{Name}={(Value ? TextColor.BrightGreen : TextColor.Red)}{Value.ToString().ToLowerInvariant()}";
		}
	}
}