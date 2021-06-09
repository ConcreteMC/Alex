using System;

namespace Alex.Common.Graphics
{
	[Flags]
	public enum VertexFlags : byte
	{
		None = 0,
		Solid = (1 << 1),
		Transparent = (1 << 2),
		FullCube = (1 << 3),
		
		Default = Solid | Transparent | FullCube
	}
}