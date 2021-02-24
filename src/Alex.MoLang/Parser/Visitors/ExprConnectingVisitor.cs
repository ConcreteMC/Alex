
using System.Collections.Generic;

namespace Alex.MoLang.Parser.Visitors
{
	public class ExprConnectingVisitor : ExprVisitor
	{
		public readonly LinkedList<IExpression> Stack = new LinkedList<IExpression>();
		public          IExpression             Previous;

		/// <inheritdoc />
		public override void BeforeTraverse(List<IExpression> expressions)
		{
			Stack.Clear();
			Previous = null;
		}

		/// <inheritdoc />
		public override object OnVisit(IExpression expression)
		{
			if (Stack.Count > 0) {
				expression.Attributes["parent"] = Stack.Last;
			}

			if (Previous != null && expression.Attributes.TryGetValue("parent", out var eParent)
			                     && Previous.Attributes.TryGetValue("parent", out var prevParent)
			                     && eParent == prevParent)
			{
				expression.Attributes["previous"] = Previous;
				expression.Attributes["next"] = expression;
			}

			Stack.AddLast(expression);

			return null;
		}

		/// <inheritdoc />
		public override void OnLeave(IExpression expression)
		{
			Previous = expression;
			Stack.RemoveLast();
		}
	}
}