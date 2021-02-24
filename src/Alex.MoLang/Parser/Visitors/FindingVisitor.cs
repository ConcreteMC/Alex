using System;
using System.Collections.Generic;

namespace Alex.MoLang.Parser.Visitors
{
	public class FindingVisitor : ExprVisitor
	{
		private Predicate<IExpression> _predicate;
		public  List<IExpression>      FoundExpressions = new List<IExpression>();

		public FindingVisitor(Predicate<IExpression> predicate) {
			_predicate = predicate;
		}

		/// <inheritdoc />
		public override object OnVisit(IExpression expression)
		{
			if (_predicate(expression))
			{
				FoundExpressions.Add(expression);
			}

			return null;
		}
	}
}