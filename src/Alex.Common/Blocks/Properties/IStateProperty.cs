using System;

namespace Alex.Common.Blocks.Properties
{
	public interface IStateProperty : IEquatable<IStateProperty>
	{
		string Name { get; }
		string StringValue { get; }
		int Identifier { get; }

		string ToFormattedString();

		IStateProperty WithValue(object value);
	}

	public interface IStateProperty<TType> : IStateProperty
	{
		TType Value { get; }

		TType ParseValue(string value);

		/// <inheritdoc />
		string IStateProperty.StringValue => Value.ToString();

		IStateProperty<TType> WithValue(TType value);

		IStateProperty IStateProperty.WithValue(object value)
		{
			if (value is TType val)
			{
				return WithValue(val);
			}

			return WithValue(ParseValue(value.ToString()));
		}
	}
}