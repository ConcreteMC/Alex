using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ResourcePackLib.CoreRT.Json
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
