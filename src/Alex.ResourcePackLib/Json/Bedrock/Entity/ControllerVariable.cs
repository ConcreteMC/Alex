using System.Collections.Generic;
using Alex.MoLang.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class ControllerVariable
	{
		[JsonProperty("input")]
		public List<IExpression> Input { get; set; }
		
		[JsonProperty("remap_curve")]
		public Dictionary<double, double> RemapCurve { get; set; }
	}
}