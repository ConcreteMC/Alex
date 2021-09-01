using System;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.Properties
{
	public class PropertyEnum<T> : StateProperty<T> where T : struct
	{
	//	private readonly T _defaultValue;

		/// <inheritdoc />
		public PropertyEnum(string name, T defaultValue) : base(name)
		{
			DefaultValue = defaultValue;
		}

		/// <inheritdoc />
		public override IStateProperty<T> WithValue(T value)
		{
			return new PropertyEnum<T>(Name, DefaultValue) {Value = value};
		}
		
		/// <inheritdoc />
		public override T ParseValue(string value)
		{
			if (Enum.TryParse(value, true, out T enumValue))
			{
				return enumValue;
			}

			return DefaultValue;
		}

		/// <inheritdoc />
		public override string StringValue => Value.ToString().ToLower();
	}
}