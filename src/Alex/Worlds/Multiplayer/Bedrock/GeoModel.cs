using System;
using System.Collections.Generic;
using System.Linq;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class GeometryModel
	{
		[JsonProperty(PropertyName = "format_version")]
		public string FormatVersion { get; set; } = "1.12.0";

		[JsonProperty("minecraft:geometry")]
		public Dictionary<string, EntityModel> Geometry { get; set; } = new Dictionary<string, EntityModel>();
	}
	
	public class GeometryModel18
	{
		[JsonProperty(PropertyName = "format_version")]
		public string FormatVersion { get; set; } = "1.8.0";

		[JsonProperty()]
		public Geometry Geometry { get; set; }
	}
}