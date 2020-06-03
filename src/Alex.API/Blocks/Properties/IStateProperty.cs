using System;

namespace Alex.API.Blocks.Properties
{
	public interface IStateProperty : IEquatable<IStateProperty>
	{
		string Name { get; }
	}

	public interface IStateProperty<TType> : IStateProperty
	{
		TType ParseValue(string value);
	}
}
