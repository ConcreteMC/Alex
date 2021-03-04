using System.Collections.Generic;

namespace Alex.MoLang.Parser
{
	public interface IExprVisitor
	{
		void BeforeTraverse(IExpression[] expressions);
		object OnVisit(IExpression expression);

		void OnLeave(IExpression expression);
		void AfterTraverse(IExpression[] expressions);
	}

	public abstract class ExprVisitor : IExprVisitor
	{
		/// <inheritdoc />
		public virtual void BeforeTraverse(IExpression[] expressions)
		{
			
		}

		/// <inheritdoc />
		public abstract object OnVisit(IExpression expression);

		/// <inheritdoc />
		public virtual void OnLeave(IExpression expression)
		{
			
		}

		/// <inheritdoc />
		public virtual void AfterTraverse(IExpression[] expressions)
		{
			
		}
	}
}