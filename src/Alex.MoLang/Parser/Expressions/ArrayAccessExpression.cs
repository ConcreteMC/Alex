using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class ArrayAccessExpression : Expression
	{
		public IExpression Array { get; set; }
		public IExpression Index { get; set; }

		public ArrayAccessExpression(IExpression array, IExpression index)
		{
			Array = array;
			Index = index;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			string name = Array is NameExpression expression ? expression.Name : Array.Evaluate(scope, environment).AsString();

			return environment.GetValue(name + "." + (int) Index.Evaluate(scope, environment).AsDouble());
		}

		/// <inheritdoc />
		public override void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			string name = Array is NameExpression expression ? expression.Name : Array.Evaluate(scope, environment).AsString();

			environment.SetValue(name + "." + (int) Index.Evaluate(scope, environment).AsDouble(), value);
		}
	}
}