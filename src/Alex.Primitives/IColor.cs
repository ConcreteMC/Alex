using System;

namespace Alex.Interfaces
{
	public interface IColor : IEquatable<IColor>
	{
		byte R { get; }
		byte G { get; }
		byte B { get; }
		byte A { get; }
	}
}