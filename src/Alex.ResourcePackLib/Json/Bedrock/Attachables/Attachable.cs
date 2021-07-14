using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Attachables
{
	public class AttachableDefinition
	{
		[JsonProperty("description")]
		public AttachableDescription Description { get; set; }
	}

	public class AttachableDescription
	{
		[JsonProperty("identifier")]
		public string Identifier { get; set; }

		[JsonProperty("min_engine_version")]
		public string MinEngineVersion { get; set; }

		[JsonProperty("item")]
		public Dictionary<string, IExpression[]> Item { get; set; }
		
		[JsonProperty("materials")]
		public Dictionary<string, string> Materials { get; set; }

		[JsonProperty("textures")]
		public Dictionary<string, string> Textures { get; set; }

		[JsonProperty("geometry")]
		public Dictionary<string, string> Geometry { get; set; }
        
		[JsonProperty("scripts")]
		public EntityScripts Scripts { get; set; }
		[JsonProperty("render_controllers")] 
		public AnnoyingMolangElement[] RenderControllers { get; set; } = new AnnoyingMolangElement[0];
	}
}