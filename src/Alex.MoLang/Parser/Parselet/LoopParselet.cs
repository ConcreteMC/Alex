using System;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class LoopParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			var args = parser.ParseArgs();

			if (args.Count != 2)
			{
				throw new Exception("Loop: Expected 2 argument, " + args.Count + " argument given");
			}
			else
			{
				return new LoopExpression(args[0], args[1]);
			}
		}
	}
}