using System.Collections.Generic;

namespace Alex.MoLang.Parser.Visitors
{
	public class ExprConnectingVisitor : ExprVisitor
	{
		//private LinkedList<IExpression> Stack { get; set; } = new LinkedList<IExpression>();

		//	private LinkedList<IExpression> _previousStack = null;

		private IExpression _last = null;

		/// <inheritdoc />
		public override void BeforeTraverse(IEnumerable<IExpression> expressions)
		{
			_last = null;
			//	_previousStack = Stack;
			//	Stack = new LinkedList<IExpression>();
		}

		/// <inheritdoc />
		public override void OnVisit(ExprTraverser traverser, IExpression expression)
		{
			var previous = _last;
			expression.Meta.Previous = previous;

			if (previous != null && previous != expression.Meta.Parent)
			{
				previous.Meta.Next = expression;
			}

			//Stack.AddLast(expression);
		}

		/// <inheritdoc />
		public override void OnLeave(IExpression expression)
		{
			_last = expression;
			//Stack.RemoveLast();
		}

		/// <inheritdoc />
		public override void AfterTraverse(IEnumerable<IExpression> expressions)
		{
			base.AfterTraverse(expressions);
			//Stack = _previousStack;
		}
	}
}