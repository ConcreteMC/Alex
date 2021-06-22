using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Sound
{
	public class SoundBindingsCollection
	{
		[JsonProperty("block_sounds")]
		public Dictionary<string, SoundBinding> BlockSounds { get; set; }
		
		[JsonProperty("entity_sounds")]
		public EntitySoundBindings EntitySounds { get; set; }
		
		[JsonProperty("individual_event_sounds")]
		public BindingBase IndividualEventSounds { get; set; }
		
		[JsonProperty("interactive_sounds")]
		public InteractiveSoundMappings InteractiveSounds { get; set; }
	}

	public class EntitySoundBindings
	{
		[JsonProperty("defaults")]
		public SoundBinding Defaults { get; set; }

		[JsonProperty("entities")]
		public Dictionary<string, SoundBinding> Entities { get; set; }
	}
	
	public partial class SoundBinding : BindingBase
	{
		[JsonProperty("pitch"), JsonConverter(typeof(SingleOrArrayConverter<double>))] public double[] Pitch { get; set; } = new double[] {1.0};

		[JsonProperty("volume")] public double Volume { get; set; } = 1.0;
	}

	[JsonConverter(typeof(SoundEventConverter))]
	public class SoundEvent
	{
		[JsonProperty("sound")] public string Sound { get; set; } = null;

		[JsonProperty("pitch"), JsonConverter(typeof(SingleOrArrayConverter<double>))]
		public double[] Pitch { get; set; } = new double[] {1.0};

		[JsonProperty("volume")] public double Volume { get; set; } = 1.0;
	}

	public class BindingBase
	{
		[JsonProperty("events")] public Dictionary<string, SoundEvent> Events { get; set; }
	}

	public class InteractiveSoundMappings
	{
		[JsonProperty("block_sounds")]
		public Dictionary<string, SoundBinding> BlockSounds { get; set; }
		
		[JsonProperty("entity_sounds")]
		public EntitySoundBindings EntitySounds { get; set; }
	}
}