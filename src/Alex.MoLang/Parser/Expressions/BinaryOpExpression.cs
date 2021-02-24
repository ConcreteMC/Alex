using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public abstract class BinaryOpExpression : Expression<IExpression>
	{
		protected IExpression Left;
		protected IExpression Right;

		protected BinaryOpExpression(IExpression l, IExpression r) : base(null)
		{
			Left = l;
			Right = r;
		}

		public abstract string GetSigil();
	}
}