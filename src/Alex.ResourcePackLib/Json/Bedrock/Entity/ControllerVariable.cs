using System.Collections.Generic;
using ConcreteMC.MolangSharp.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	public class ControllerVariable
	{
		[JsonProperty("input")] public IExpression Input { get; set; }

		[JsonProperty("remap_curve")] public Dictionary<double, double> RemapCurve { get; set; }
	}
}