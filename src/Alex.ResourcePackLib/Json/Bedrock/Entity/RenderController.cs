using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class RenderController
	{
		[JsonProperty("geometry")] public string Geometry { get; set; } = null;

		[JsonProperty("rebuild_animation_matrices")]
		public bool RebuildAnimationMatrices { get; set; } = false;

		[JsonProperty("part_visibility")]
		public AnnoyingMolangElement[] PartVisibility { get; set; } = new AnnoyingMolangElement[]
		{
			new AnnoyingMolangElement(new Dictionary<string, IExpression[]>()
			{
				{"*", new IExpression[] {new BooleanExpression(true)}}
			})
		};

		[JsonProperty("materials")] public AnnoyingMolangElement[] Materials { get; set; } = new AnnoyingMolangElement[]
		{
			new AnnoyingMolangElement(new Dictionary<string, IExpression[]>()
			{
				{"*", new IExpression[]
				{
					new StringExpression("Material.default")
				}}
			})
		};

		[JsonProperty("textures")]
		public AnnoyingMolangElement[] Textures { get; set; } = new AnnoyingMolangElement[]
		{
			new AnnoyingMolangElement("Texture.default")
		};
		
		[JsonProperty("arrays")]
		public Dictionary<string, IDictionary<string, IExpression[]>> Arrays { get; set; }
	}

	public class PartVisibility : Dictionary<string, List<IExpression>>
	{
		
	}
	
	//public class Materials : AnnoyingMolangElement
	//{
		
	//}
}