using System.Collections.Generic;
using Alex.MoLang.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class AnimationKeyFrame
	{
		/// <summary>
		///		when interpolating towards this key frame from the previous, use this value
		/// </summary>
		[JsonProperty("pre")]
		public IExpression[][] Pre { get; set; }

		/// <summary>
		///  when at interpolating away from this key frame to the next, use this value
		/// </summary>
		[JsonProperty("post")]
		public IExpression[][] Post { get; set; }
	}
}