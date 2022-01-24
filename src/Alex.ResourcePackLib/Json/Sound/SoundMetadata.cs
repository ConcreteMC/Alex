using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Sound
{
	public partial class SoundMetadata
	{
		[JsonProperty("name")] public string Name { get; set; }

		[JsonProperty("stream", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Stream { get; set; }

		[JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
		public double? Volume { get; set; }

		[JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
		public long? Weight { get; set; }

		[JsonProperty("pitch", NullValueHandling = NullValueHandling.Ignore)]
		public double? Pitch { get; set; }

		[JsonProperty("attenuation_distance", NullValueHandling = NullValueHandling.Ignore)]
		public double? AttenuationDistance { get; set; }

		[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
		public SoundType? Type { get; set; }
	}
}