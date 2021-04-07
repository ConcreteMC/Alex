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
        public static List<IExpression> Find(Predicate<IExpression> predicate, params IExpression[] expressions) {
            ExprTraverser  traverser = new ExprTraverser();
            FindingVisitor visitor   = new FindingVisitor(predicate);

            traverser.Visitors.Add(visitor);
            traverser.Traverse(expressions);

            return visitor.FoundExpressions;
        }

        public static IExpression FindFirst(Predicate<IExpression> predicate, params IExpression[] expressions) {
            ExprTraverser       traverser = new ExprTraverser();
            FirstFindingVisitor visitor   = new FirstFindingVisitor(predicate);

            traverser.Visitors.Add(visitor);
            traverser.Traverse(expressions);

            return visitor.Found;
        }
    }
    public class ExprTraverser
    {
        private bool _stopTraversal = false;

        public readonly List<IExprVisitor> Visitors = new List<IExprVisitor>();

        public IEnumerable<IExpression> Traverse(IExpression[] expressions)
        {
            foreach (IExprVisitor visitor in Visitors) {
                visitor.BeforeTraverse(expressions);
            }

            _stopTraversal = false;

            foreach (var expression in TraverseArray(expressions))
            {
                yield return expression;
            }
            //TraverseArray(expressions);

            foreach (IExprVisitor visitor in Visitors) {
                visitor.AfterTraverse(expressions);
            }

           // return expressions;
        }

        private IEnumerable<IExpression> TraverseArray(IExpression[] expressions)
        {
            //var list = expressions.ToList();

            //for (var i = 0; i < list.Count; i++)
            for (var index = 0; index < expressions.Length; index++)
            {
                IExpression expression = expressions[index];
                
                if (expression == null)
                    throw new MoLangRuntimeException("Expression was null", null);

                var removeCurrent = false;
                var traverseChildren = true;
                var traverseCurrent = true;

                foreach (var visitor in Visitors)
                {
                    var result = visitor.OnVisit(expression);

                    if (result is ActionType at)
                    {
                        switch (at)
                        {
                            case ActionType.RemoveCurrent:
                                removeCurrent = true;

                                break;

                            case ActionType.StopTraversal:
                                _stopTraversal = true;

                                break;

                            case ActionType.DontTraverseCurrentAndChildren:
                                traverseCurrent = false;
                                traverseChildren = false;

                                break;

                            case ActionType.DontTraverseChildren:
                                traverseChildren = false;

                                break;
                        }
                    }
                    else if (result is IExpression result1)
                    {
                        expression = result1;
                    }
                }

                if (!traverseCurrent)
                {
                    break;
                }

                if (traverseChildren && !removeCurrent)
                {
                    expression = TraverseExpr(expression);
                }

                foreach (IExprVisitor visitor in Visitors)
                {
                    visitor.OnLeave(expression);
                }

                if (removeCurrent)
                {
                    //list.Remove(expression);
                    expressions[index] = null;//.Remove(expression);
                }
                else
                {
                    expressions[index] = expression;

                    yield return expression;
                    //expressions[i] = expression;//.set(i, expression);
                }

                if (_stopTraversal)
                {
                    break;
                }
            }

            //return expressions.Where(x => x != null).ToArray();
        }

        private IExpression TraverseExpr(IExpression expression)
        {
            foreach (var field in GetAllProperties(expression.GetType()))
            {
                //field.setAccessible(true);
                var fieldValue = GetFieldValue(field, expression);

                if (fieldValue is IExpression subExpr)
                {
                    var removeCurrent    = false;
                    var traverseChildren = true;
                    var traverseCurrent = true;

                    foreach (var visitor in Visitors)
                    {
                        var result = visitor.OnVisit(subExpr);

                        if (result is ActionType at)
                        {
                            switch (at)
                            { 
                                case ActionType.RemoveCurrent: 
                                    removeCurrent = true; 
                                    break;

                                case ActionType.StopTraversal:
                                    _stopTraversal = true;

                                    break;

                                case ActionType.DontTraverseCurrentAndChildren:
                                    traverseCurrent = false;
                                    traverseChildren = false;

                                    break;

                                case ActionType.DontTraverseChildren:
                                    traverseChildren = false;

                                    break;
                            }
                        }
                        else if (result is IExpression result1)
                        {
                            subExpr = result1;
                        }
                    }

                    if (!traverseCurrent)
                    {
                        break;
                    }

                    if (traverseChildren && !removeCurrent)
                    {
                        subExpr = TraverseExpr(subExpr);
                    }

                    foreach (var visitor in Visitors)
                    {
                        visitor.OnLeave(subExpr);
                    }

                    if (removeCurrent)
                    {
                        SetFieldValue(field, expression, null);
                    }
                    else
                    {
                        if (subExpr != fieldValue) 
                            SetFieldValue(field, expression, subExpr);
                    }

                    if (_stopTraversal)
                    {
                        break;
                    }
                }
                else if (fieldValue != null && fieldValue.GetType().IsArray)
                {
                    var array = (object[]) fieldValue;
                    var exprs = array.Where(x => x is IExpression).Cast<IExpression>().ToArray();

                    exprs = TraverseArray(exprs).ToArray();
                    
                    SetFieldValue(field, expression, exprs);
                }
            }

            return expression;
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
                return field.GetValue(obj);//.get(obj);
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
            try
            {
                
            }
            catch (Exception throwable)
            {
                
                // noop
            }
        }

        public enum ActionType
        {
            RemoveCurrent,
            StopTraversal,
            DontTraverseCurrentAndChildren,
            DontTraverseChildren
        }
    }
}