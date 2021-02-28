using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alex.MoLang.Parser;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class Animation
	{
		[JsonProperty("loop")]
		public bool Loop { get; set; } = false;

		[JsonProperty("bones")]
		public Dictionary<string, AnimationBoneElement> Bones { get; set; } =
			new Dictionary<string, AnimationBoneElement>();
	}

	public class AnimationBoneElement
	{
		[JsonProperty("position"), JsonConverter(typeof(SingleOrArrayConverter<List<IExpression>>))]
		public List<IExpression>[] Position { get; set; }
		
		[JsonProperty("rotation"), JsonConverter(typeof(SingleOrArrayConverter<List<IExpression>>))]
		public List<IExpression>[] Rotation { get; set; }
	}
}