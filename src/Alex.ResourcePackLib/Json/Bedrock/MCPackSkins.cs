namespace Alex.ResourcePackLib.Json.Bedrock
{
	using System;
	using System.Collections.Generic;

	using System.Globalization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public class MCPackSkins
	{
		[JsonProperty("skins")]
		public SkinEntry[] Skins { get; set; }

		[JsonProperty("serialize_name")]
		public string SerializeName { get; set; }

		[JsonProperty("localization_name")]
		public string LocalizationName { get; set; }
	}

	public class SkinEntry
	{
		[JsonProperty("localization_name")]
		public string LocalizationName { get; set; }

		[JsonProperty("geometry")]
		public string Geometry { get; set; }

		[JsonProperty("texture")]
		public string Texture { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }
	}
}