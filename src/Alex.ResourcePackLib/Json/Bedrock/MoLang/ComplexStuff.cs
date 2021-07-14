using Alex.MoLang.Parser;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Converters.Bedrock;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.MoLang
{
	[JsonConverter(typeof(ComplexStuffConverter))]
	public class ComplexStuff
	{
		public IExpression[][] Expressions { get; set; }
		public PrePostKeyFrame Frame { get; set; }
	}
}