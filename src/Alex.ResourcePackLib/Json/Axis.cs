using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Axis
	{
		Undefined,
		X,
		Y,
		Z
	}
}
