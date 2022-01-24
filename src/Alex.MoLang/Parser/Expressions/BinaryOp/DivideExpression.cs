using System;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class DivideExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public DivideExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			try
			{
				return new DoubleValue(
					Left.Evaluate(scope, environment).AsDouble() / Right.Evaluate(scope, environment).AsDouble());
			}
			catch (Exception ex)
			{
				throw new MoLangRuntimeException("An unexpected error occured.", ex);
			}
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "/";
		}
	}
}