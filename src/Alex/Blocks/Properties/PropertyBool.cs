using System;
using Alex.Blocks.State;
using Alex.Common.Utils;
using MiNET.Utils;

namespace Alex.Blocks.Properties
{
	public class PropertyBool : StateProperty<bool>
	{
		public static readonly PropertyBool NORTH = new PropertyBool("north");
		public static readonly PropertyBool EAST = new PropertyBool("east");
		public static readonly PropertyBool SOUTH = new PropertyBool("south");
		public static readonly PropertyBool WEST = new PropertyBool("west");
		public static readonly PropertyBool UP = new PropertyBool("up");
		public static readonly PropertyBool DOWN = new PropertyBool("down");
		
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
		public override string StringValue => Value ? TrueString : FalseString;

		/// <inheritdoc />
		public override StateProperty<bool> WithValue(bool value)
		{
			return new PropertyBool(Name, TrueString, FalseString) {Value = value};
		}

		public override bool ParseValue(string value)
		{
			if (bool.TryParse(value, out var result))
			{
				return result;
			}

			if (string.Equals(value, TrueString, StringComparison.InvariantCultureIgnoreCase) )
			{
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public override string ToFormattedString()
		{
			return $"{Name}={(Value ? TextColor.BrightGreen : TextColor.Red)}{(Value ? TrueString : FalseString)}";
		}
	}
}
