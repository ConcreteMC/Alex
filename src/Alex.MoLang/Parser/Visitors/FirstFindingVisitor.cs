using System;

namespace Alex.MoLang.Parser.Visitors
{
	public class FirstFindingVisitor : ExprVisitor
	{
		private Predicate<IExpression> _predicate;
		public IExpression Found = null;

		public FirstFindingVisitor(Predicate<IExpression> predicate)
		{
			_predicate = predicate;
		}

		/// <inheritdoc />
		public override void OnVisit(ExprTraverser traverser, IExpression expression)
		{
			if (_predicate(expression))
			{
				Found = expression;

				traverser.Stop();
			}
		}
	}
}