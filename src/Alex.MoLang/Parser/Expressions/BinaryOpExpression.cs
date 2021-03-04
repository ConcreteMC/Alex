namespace Alex.MoLang.Parser.Expressions
{
	public abstract class BinaryOpExpression : Expression
	{
		public IExpression Left { get; set; }
		public IExpression Right { get; set; }

		protected BinaryOpExpression(IExpression l, IExpression r)
		{
			Left = l;
			Right = r;
		}

		public abstract string GetSigil();
	}
}