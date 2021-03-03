using System.Collections.Generic;
using Alex.MoLang.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class PrePostKeyFrame
	{
		[JsonProperty("pre")]
		public List<IExpression>[] Pre { get; set; }
		
		[JsonProperty("post")]
		public List<IExpression>[] Post { get; set; }
	}
}