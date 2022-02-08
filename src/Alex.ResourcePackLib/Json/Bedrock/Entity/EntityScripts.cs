using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using ConcreteMC.MolangSharp.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class EntityScripts
	{
		[JsonProperty("initialize")] public IExpression[] Initialize { get; set; } = new IExpression[0];

		[JsonProperty("pre_animation")] public IExpression[] PreAnimation { get; set; } = new IExpression[0];

		[JsonProperty("scale")] public IExpression Scale { get; set; } = null;

		[JsonProperty("parent_setup")] public IExpression ParentSetup { get; set; } = null;

		[JsonProperty("animate")] public AnnoyingMolangElement[] Animate { get; set; } = null;

		[JsonProperty("should_update_bones_and_effects_offscreen")]
		public IExpression ShouldUpdateBonesAndEffectsOffscreen { get; set; } = null;

		[JsonProperty("should_update_effects_offscreen")]
		public IExpression ShouldUpdateEffectsOffscreen { get; set; } = null;
	}
}