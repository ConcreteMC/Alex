using System;
using System.Collections.Generic;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class ForEachParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			List<IExpression> args = parser.ParseArgs();

			if (args.Count != 3)
			{
				throw new Exception("ForEach: Expected 3 argument, " + args.Count + " argument given");
			}
			else
			{
				return new ForEachExpression(args[0], args[1], args[2]);
			}
		}
	}
}