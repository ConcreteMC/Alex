using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using ConcreteMC.MolangSharp.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.MoLang
{
	[JsonConverter(typeof(AnimationChannelDataConverter))]
	public class AnimationChannelData
	{
		public IExpression[] Expressions { get; set; }
		public AnimationKeyFrame KeyFrame { get; set; }
	}
}