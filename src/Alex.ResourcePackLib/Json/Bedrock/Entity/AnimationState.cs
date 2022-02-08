using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class AnimationState
	{
		[JsonProperty("animations")] public AnnoyingMolangElement[] Animations { get; set; }

		[JsonProperty("transitions")] public AnnoyingMolangElement[] Transitions { get; set; }

		[JsonProperty("remap_curve")] public Dictionary<string, ControllerVariable> Variables { get; set; }
	}
}