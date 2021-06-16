using System;

namespace Alex.Common.Blocks.Properties
{
	public interface IStateProperty : IEquatable<IStateProperty>
	{
		string Name { get; }
		string StringValue { get; }
	}

	public interface IStateProperty<TType> : IStateProperty
	{
		TType Value { get; }
		TType ParseValue(string value);

		/// <inheritdoc />
		string IStateProperty.StringValue => Value.ToString();
	}
}
