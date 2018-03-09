using System;

namespace Alex.API.Blocks.Properties
{
	public interface IStateProperty
	{
		string Name { get; }
		Type PropertyType { get; }
		object ValueFromString(string value);
	}

	public interface IStateProperty<TType> : IStateProperty
	{
		TType ParseValue(string value);
	}
}
