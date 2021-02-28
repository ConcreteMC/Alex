using System.Collections.Generic;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class AnimationController
	{
		[JsonProperty("states")]
		public Dictionary<string, AnimationState> States { get; set; } = new Dictionary<string, AnimationState>();

		[JsonProperty("initial_state")]
		public string InitialState { get; set; } = null;
	}

	public class AnimationState
	{
		[JsonProperty("animations")]
		public AnnoyingMolangElement[] Animations { get; set; }
		
		[JsonProperty("transitions")]
		public AnnoyingMolangElement[] Transitions { get; set; }
	}
}