using System;

namespace Alex.API.Blocks.Properties
{
	public interface IStateProperty : IEquatable<IStateProperty>
	{
		string Name { get; }
		Type PropertyType { get; }
		object ValueFromString(string value);

		object[] GetValidValues();
		object DefaultValue { get; }
	}

	public interface IStateProperty<TType> : IStateProperty
	{
		TType ParseValue(string value);
		string ToString(TType v);
		TType GetDefaultValue();
	}
}
