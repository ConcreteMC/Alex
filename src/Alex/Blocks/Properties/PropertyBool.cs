using System;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;
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
		protected override string StringifyValue(bool value)
		{
			return value ? TrueString : FalseString;
		}

		/// <inheritdoc />
		public override IStateProperty<bool> WithValue(bool value)
		{
			return new PropertyBool(Name, TrueString, FalseString) {Value = value};
		}

		public override bool ParseValue(string value)
		{
			return string.Equals(value, TrueString, StringComparison.OrdinalIgnoreCase);
		}

		/// <inheritdoc />
		public override string ToFormattedString()
		{
			return $"{Name}={(Value ? TextColor.BrightGreen : TextColor.Red)}{(Value ? TrueString : FalseString)}";
		}
	}
}
