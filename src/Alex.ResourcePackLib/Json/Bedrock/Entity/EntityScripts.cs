using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class EntityScripts
	{
		[JsonProperty("initialize")] 
		public IExpression[][] Initialize { get; set; } = new IExpression[0][];

		[JsonProperty("pre_animation")] 
		public IExpression[][] PreAnimation { get; set; } = new IExpression[0][];

		[JsonProperty("scale")] 
		public IExpression[] Scale { get; set; } = null;

		[JsonProperty("parent_setup")]
		public IExpression[] ParentSetup { get; set; } = null;

		[JsonProperty("animate")] 
		public AnnoyingMolangElement[] Animate { get; set; } = new AnnoyingMolangElement[0];
		
		[JsonProperty("should_update_bones_and_effects_offscreen")]
		public IExpression[] ShouldUpdateBonesAndEffectsOffscreen { get; set; } = null;
		
		[JsonProperty("should_update_effects_offscreen")]
		public IExpression[] ShouldUpdateEffectsOffscreen { get; set; } = null;
	}
	
	public class AnnoyingMolangElement
	{
		public Dictionary<string, IExpression[]> Expressions;
		public string                                StringValue;

		public AnnoyingMolangElement(Dictionary<string, IExpression[]> expressions)
		{
			Expressions = expressions;
			StringValue = null;

			IsString = false;
		}

		public AnnoyingMolangElement(string stringValue)
		{
			StringValue = stringValue;
			Expressions = null;

			IsString = true;
		}
		
		public bool IsString { get; }

		//	public static implicit operator AnnoyingMolangElement(Dictionary<string, IExpression[]> dictionary) => new AnnoyingMolangElement { Expressions = dictionary };
	//	public static implicit operator AnnoyingMolangElement(string stringValue) => new AnnoyingMolangElement { StringValue = stringValue };
	}
}