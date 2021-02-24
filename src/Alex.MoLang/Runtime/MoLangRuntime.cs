using System.Collections.Generic;
using System.Linq.Expressions;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Visitors;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime
{
	public class MoLangRuntime
	{
		public MoLangEnvironment Environment { get; } = new MoLangEnvironment();

		public MoLangRuntime()
		{
			Environment.Structs.TryAdd("math", MoLangMath.Library);
			Environment.Structs.TryAdd("temp", new VariableStruct());
			Environment.Structs.TryAdd("variable", new VariableStruct());
			Environment.Structs.TryAdd("array", new ArrayStruct());
		}
		
		public IMoValue Execute(IExpression expression) {
			return Execute(new List<IExpression>()
			{
				expression
			});
		}

		public IMoValue Execute(List<IExpression> expressions) {
			return Execute(expressions, new Dictionary<string, IMoValue>());
		}

		public IMoValue Execute(List<IExpression> expressions, Dictionary<string, IMoValue> context) {
			ExprTraverser traverser = new ExprTraverser();
			traverser.Visitors.Add(new ExprConnectingVisitor());
			traverser.Traverse(expressions);

			Environment.Structs["context"] = new ContextStruct(context);// .put("context", new ContextStruct(context));

			IMoValue result = new DoubleValue(0.0);
			MoScope scope  = new MoScope();
			foreach (IExpression expression in new List<IExpression>(expressions)) {
				if (scope.ReturnValue != null) {
					break;
				}
				result = expression.Evaluate(scope, Environment);
			}

			Environment.Structs["temp"].Clear();;// .getStructs().get("temp").clear();
			Environment.Structs.TryRemove("context", out _);//["context"].getStructs().remove("context");

			return scope.ReturnValue != null ? scope.ReturnValue : result;
		}
	}
}