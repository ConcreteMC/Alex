using System;
using System.Collections.Generic;
using System.Reflection;
using Alex.MoLang.Parser.Visitors;

namespace Alex.MoLang.Parser
{
    public static class ExprFinder
    {
        public static List<IExpression> Find(List<IExpression> expressions, Predicate<IExpression> predicate) {
            ExprTraverser  traverser = new ExprTraverser();
            FindingVisitor visitor   = new FindingVisitor(predicate);

            traverser.Visitors.Add(visitor);
            traverser.Traverse(expressions);

            return visitor.FoundExpressions;
        }

        public static IExpression FindFirst(List<IExpression> expressions, Predicate<IExpression> predicate) {
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

        public void Traverse(List<IExpression> expressions)
        {
            foreach (IExprVisitor visitor in Visitors) {
                visitor.BeforeTraverse(expressions);
            }

            _stopTraversal = false;
            TraverseArray(expressions);

            foreach (IExprVisitor visitor in Visitors) {
                visitor.AfterTraverse(expressions);
            }
        }

        private void TraverseArray(List<IExpression> expressions)
        {
            var list = new List<IExpression>(expressions);

            for (var i = 0; i < list.Count; i++)
            {
                var expression = list[i];

                var removeCurrent    = false;
                var traverseChildren = true;
                var traverseCurrent  = true;

                foreach (var visitor in Visitors) {
                    var result = visitor.OnVisit(expression);

                    if (result is ActionType at) {
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
                    } else if (result is IExpression) {
                        expression = (IExpression) result;
                    }
                }

                if (!traverseCurrent)
                {
                    break;
                }
                else if (traverseChildren && !removeCurrent)
                {
                    TraverseExpr(expression);
                }

                foreach (IExprVisitor visitor in Visitors) {
                    visitor.OnLeave(expression);
                }

                if (removeCurrent)
                {
                    expressions.Remove(expression);
                }
                else
                {
                    expressions[i] = expression;//.set(i, expression);
                }

                if (_stopTraversal)
                {
                    break;
                }
            }
        }

        private void TraverseExpr(IExpression expression)
        {
            foreach (var field in GetAllFields(expression.GetType())) {
                //field.setAccessible(true);
                var fieldValue = GetFieldValue(field, expression);

                if (fieldValue is IExpression) {
                    var subExpr = (IExpression) fieldValue;

                    var removeCurrent    = false;
                    var traverseChildren = true;
                    var traverseCurrent  = true;

                    foreach (var visitor in
                    Visitors) {
                        var result = visitor.OnVisit(subExpr);

                        if (result is ActionType at) {
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
                        } else if (result is IExpression)
                        {
                            subExpr = (IExpression) result;
                        }
                    }

                    if (!traverseCurrent)
                    {
                        break;
                    }
                    else if (traverseChildren && !removeCurrent)
                    {
                        TraverseExpr(subExpr);
                    }

                    foreach (var visitor in Visitors) {
                        visitor.OnLeave(subExpr);
                    }

                    if (removeCurrent)
                    {
                        SetFieldValue(field, expression, null);
                    }
                    else
                    {
                        SetFieldValue(field, expression, subExpr);
                    }

                    if (_stopTraversal)
                    {
                        break;
                    }
                } else if (fieldValue != null && fieldValue.GetType().IsArray)
                {
                    var         array = (object[]) fieldValue;
                    var exprs = new List<IExpression>();

                    foreach (var i in array) {
                        if (i is IExpression) {
                            exprs.Add((IExpression) i);
                        }
                    }

                    TraverseArray(exprs);

                    SetFieldValue(field, expression, exprs.ToArray());
                }
            }
        }

        public static List<FieldInfo> GetAllFields(Type type)
        {
            var fields = new List<FieldInfo>();

            foreach (var field in type.GetFields())
            {
                fields.Add(field);
            }

            return fields;
        }

        private Object GetFieldValue(FieldInfo field, object obj)
        {
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

        private void SetFieldValue(FieldInfo field, object obj, object value)
        {
            try
            {
                field.SetValue(obj, value);
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