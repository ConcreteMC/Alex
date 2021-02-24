using System.Collections.Generic;

namespace Alex.MoLang.Parser
{
	public interface IExprVisitor
	{
		void BeforeTraverse(List<IExpression> expressions);
		object OnVisit(IExpression expression);

		void OnLeave(IExpression expression);
		void AfterTraverse(List<IExpression> expressions);
	}

	public abstract class ExprVisitor : IExprVisitor
	{
		/// <inheritdoc />
		public virtual void BeforeTraverse(List<IExpression> expressions)
		{
			
		}

		/// <inheritdoc />
		public abstract object OnVisit(IExpression expression);

		/// <inheritdoc />
		public virtual void OnLeave(IExpression expression)
		{
			
		}

		/// <inheritdoc />
		public virtual void AfterTraverse(List<IExpression> expressions)
		{
			
		}
	}
}