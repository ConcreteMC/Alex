using System.Collections.Generic;
using Alex.MoLang.Parser;

namespace Alex.ResourcePackLib.Json.Bedrock.MoLang
{
	public class AnnoyingMolangElement
	{
		public Dictionary<string, IExpression[]> Expressions;
		public string                            StringValue;

		public AnnoyingMolangElement(Dictionary<string, IExpression[]> expressions)
		{
			Expressions = expressions;
			StringValue = null;

			IsString = false;
		}

		public AnnoyingMolangElement(string stringValue)
		{
			StringValue = stringValue;
			Expressions = null;

			IsString = true;
		}
		
		public bool IsString { get; }

		//	public static implicit operator AnnoyingMolangElement(Dictionary<string, IExpression[]> dictionary) => new AnnoyingMolangElement { Expressions = dictionary };
		//	public static implicit operator AnnoyingMolangElement(string stringValue) => new AnnoyingMolangElement { StringValue = stringValue };
	}
}