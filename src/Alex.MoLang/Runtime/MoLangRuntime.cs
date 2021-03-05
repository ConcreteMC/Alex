using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Visitors;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime
{
	public class MoLangRuntime
	{
		public MoLangEnvironment Environment { get; } = new MoLangEnvironment();

		private ExprTraverser _exprTraverser;
		public MoLangRuntime()
		{
			Environment.Structs.TryAdd("math", MoLangMath.Library);
			Environment.Structs.TryAdd("temp", new VariableStruct());
			Environment.Structs.TryAdd("variable", new VariableStruct());
			Environment.Structs.TryAdd("array", new ArrayStruct());
			
			_exprTraverser = new ExprTraverser();
			_exprTraverser.Visitors.Add(new ExprConnectingVisitor());
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

		public IMoValue Execute(IExpression[] expressions, IDictionary<string, IMoValue> context) {
			//try
			//{
				
				//expressions = _exprTraverser.Traverse(expressions);

				Environment.Structs["context"] =
					new ContextStruct(context); // .put("context", new ContextStruct(context));

				IMoValue result = new DoubleValue(0.0);
				MoScope scope = new MoScope();

				foreach (IExpression expression in expressions)
				{
					result = expression.Evaluate(scope, Environment);

					if (scope.ReturnValue != null)
					{
						result = scope.ReturnValue;

						break;
					}
				}

				Environment.Structs["temp"].Clear();
				; // .getStructs().get("temp").clear();
				Environment.Structs.Remove("context", out _); //["context"].getStructs().remove("context");

				return result;
		//	}
			//catch (Exception ex)
			//{
			//	throw new MoLangRuntimeException("An unexpected error occured.", ex);
			//}
		}
	}
}