using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum BlockFace : int
	{
		Down = 0,
		Up = 1,
		East = 2,
		West = 3,
		North = 4,
		South = 5,
		None = 255,
	}
}
