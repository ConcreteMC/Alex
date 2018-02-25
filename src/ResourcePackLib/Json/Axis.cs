using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ResourcePackLib.Json
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
