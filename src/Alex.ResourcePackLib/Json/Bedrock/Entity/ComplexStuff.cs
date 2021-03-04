using System.Collections.Generic;
using Alex.MoLang.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	[JsonConverter(typeof(ComplexStuffConverter))]
	public class ComplexStuff
	{
		public IExpression[][] Expressions { get; set; }
		public PrePostKeyFrame Frame { get; set; }
	}
}