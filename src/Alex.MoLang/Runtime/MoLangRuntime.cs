using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Visitors;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using NLog;

namespace Alex.MoLang.Runtime
{
	public class MoLangRuntime
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MoLangRuntime));
		public MoLangEnvironment Environment { get; } = new MoLangEnvironment();


		public MoLangRuntime()
		{
			Environment.Structs.TryAdd("math", MoLangMath.Library);
			Environment.Structs.TryAdd("temp", new VariableStruct());
			Environment.Structs.TryAdd("variable", new VariableStruct());
			Environment.Structs.TryAdd("array", new ArrayStruct());

			Environment.Structs.TryAdd("context", new ContextStruct());
		}
		
		/*public IMoValue Execute(IExpression expression) {
			return Execute(new List<IExpression>()
			{
				expression
			});
		}*/

		public IMoValue Execute(params IExpression[] expressions) {
			return Execute(expressions, null);
		}

		public IMoValue Execute(IExpression[] expressions, IDictionary<string, IMoValue> context)
		{
			if (expressions == null)
				return DoubleValue.Zero;
			//try
			//{

			//expressions = _exprTraverser.Traverse(expressions);

			if (Environment.Structs.TryGetValue("context", out IMoStruct cont) && cont is ContextStruct contextStruct)
			{
				contextStruct.Container = context;
			}

			IMoValue result = new DoubleValue(0.0);
			MoScope scope = new MoScope();

			foreach (IExpression expression in expressions)
			{
				if (expression == null)
					continue;

				try
				{
					result = expression.Evaluate(scope, Environment);

					if (scope.ReturnValue != null)
					{
						result = scope.ReturnValue;

						break;
					}
				}
				catch (Exception ex)
				{
					throw new MoLangRuntimeException(
						expression, "An error occured while evaluating the expression", ex);
					break;
				}
			}

			Environment.Structs["temp"].Clear();
			//Environment.Structs.Remove("context", out _);

			return result;
		}
	}
}