
using System.Collections.Generic;

namespace Alex.MoLang.Parser.Visitors
{
	public class ExprConnectingVisitor : ExprVisitor
	{
		private LinkedList<IExpression> Stack { get; set; } = new LinkedList<IExpression>();
		private IExpression Previous { get; set; }

		/// <inheritdoc />
		public override void BeforeTraverse(IEnumerable<IExpression> expressions)
		{
			Stack.Clear();
			Previous = null;
		}

		/// <inheritdoc />
		public override object OnVisit(IExpression expression)
		{
			if (Stack.Count > 0) {
				expression.Meta.Parent = Stack.Last.Value;// .Attributes["parent"] = Stack.Last;
			}

			if (Previous != null && expression.Meta.Parent != null
			                     && Previous.Meta.Parent != null
			                     && expression.Meta.Parent == Previous.Meta.Parent)
			{
				expression.Meta.Previous = Previous;// .Attributes["previous"] = Previous;
				Previous.Meta.Next = expression;// .Attributes["next"] = expression;
			}

			Stack.AddLast(expression);

			return expression;
		}

		/// <inheritdoc />
		public override void OnLeave(IExpression expression)
		{
			Previous = expression;
			Stack.RemoveLast();
		}
	}
}