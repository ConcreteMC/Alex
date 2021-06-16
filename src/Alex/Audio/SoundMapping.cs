namespace Alex.Audio
{
	using System;
	using System.Collections.Generic;

	using System.Globalization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public partial class SoundMapping
	{
		[JsonProperty("playsound_mapping")]
		public string PlaysoundMapping { get; set; }

		[JsonProperty("bedrock_mapping")]
		public string BedrockMapping { get; set; }

		[JsonProperty("identifier", NullValueHandling = NullValueHandling.Ignore)]
		public string Identifier { get; set; }

		[JsonProperty("level_event", NullValueHandling = NullValueHandling.Ignore)]
		public bool? LevelEvent { get; set; }

		[JsonProperty("extra_data", NullValueHandling = NullValueHandling.Ignore)]
		public long? ExtraData { get; set; }
	}
}