using System;

namespace Alex.Common.Blocks.Properties
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
