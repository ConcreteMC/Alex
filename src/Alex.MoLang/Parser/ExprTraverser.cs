using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alex.MoLang.Parser.Visitors;
using Alex.MoLang.Runtime.Exceptions;

namespace Alex.MoLang.Parser
{
	public static class ExprFinder
	{
		public static List<IExpression> Find(Predicate<IExpression> predicate, params IExpression[] expressions)
		{
			ExprTraverser traverser = new ExprTraverser();
			FindingVisitor visitor = new FindingVisitor(predicate);

			traverser.Visitors.Add(visitor);
			traverser.Traverse(expressions);

			return visitor.FoundExpressions;
		}

		public static IExpression FindFirst(Predicate<IExpression> predicate, params IExpression[] expressions)
		{
			ExprTraverser traverser = new ExprTraverser();
			FirstFindingVisitor visitor = new FirstFindingVisitor(predicate);

			traverser.Visitors.Add(visitor);
			traverser.Traverse(expressions);

			return visitor.Found;
		}
	}

	public class ExprTraverser
	{
		public readonly List<IExprVisitor> Visitors = new List<IExprVisitor>();

		private bool _stop = false;

		public IExpression[] Traverse(IExpression[] expressions)
		{
			TraverseArray(expressions);

			return expressions.Where(x => x != null).ToArray();
		}

		private void TraverseArray(IExpression[] expressions)
		{
			foreach (IExprVisitor visitor in Visitors)
			{
				visitor.BeforeTraverse(expressions);
			}

			for (var index = 0; index < expressions.Length; index++)
			{
				IExpression expression = expressions[index];

				if (expression == null)
					throw new MoLangRuntimeException("Expression was null", null);

				expressions[index] = TraverseExpr(expression, null);

				if (_stop)
				{
					break;
				}
			}

			foreach (IExprVisitor visitor in Visitors)
			{
				visitor.AfterTraverse(expressions);
			}

			//return expressions.Where(x => x != null).ToArray();
		}

		private IExpression TraverseExpr(IExpression expression, IExpression parent)
		{
			Visit(expression);
			expression.Meta.Parent = parent;

			foreach (var field in GetAllProperties(expression.GetType()))
			{
				if (!typeof(IEnumerable<Expression>).IsAssignableFrom(field.PropertyType)
				    && !typeof(IExpression).IsAssignableFrom(field.PropertyType))
				{
					continue;
				}

				//field.setAccessible(true);
				var fieldValue = GetFieldValue(field, expression);

				if (fieldValue == null)
					continue;

				if (fieldValue is IExpression original)
				{
					fieldValue = TraverseExpr(original, expression);
				}
				else if (fieldValue is IEnumerable<IExpression> expressions)
				{
					var exprs = expressions.ToArray();

					foreach (var ex in exprs)
					{
						if (ex != null)
							ex.Meta.Parent = expression;
					}

					TraverseArray(exprs);

					fieldValue = exprs;
				}

				SetFieldValue(field, expression, fieldValue);

				if (_stop)
				{
					break;
				}
			}

			OnLeave(expression);

			return expression;
		}

		private void Visit(IExpression expression)
		{
			//  VisitationResult visitationResult = VisitationResult.None;
			foreach (var visitor in Visitors)
			{
				visitor.OnVisit(this, expression);
			}

			// return visitationResult;
		}

		private void OnLeave(IExpression expression)
		{
			foreach (var visitor in Visitors)
			{
				visitor.OnLeave(expression);
			}
		}

		public void Stop()
		{
			_stop = true;
		}

		private static ConcurrentDictionary<Type, PropertyInfo[]> _cachedProperties =
			new ConcurrentDictionary<Type, PropertyInfo[]>();

		private static PropertyInfo[] GetAllProperties(Type type)
		{
			return _cachedProperties.GetOrAdd(
				type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray());
			//  return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
		}

		private object GetFieldValue(PropertyInfo field, object obj)
		{
			return field.GetValue(obj);

			try
			{
				return field.GetValue(obj); //.get(obj);
			}
			catch (Exception throwable)
			{
				// noop
			}

			return null;
		}

		private void SetFieldValue(PropertyInfo field, object obj, object value)
		{
			field.SetValue(obj, value);

			return;

			try { }
			catch (Exception throwable)
			{
				// noop
			}
		}

		[Flags]
		public enum VisitationResult
		{
			None,

			RemoveCurrent = 0x01,
			StopTraversal = 0x02,
			DontTraverseChildren = 0x04,
			DontTraverseCurrent = 0x08,
			DontTraverseCurrentAndChildren = DontTraverseCurrent | DontTraverseChildren
		}
	}
}