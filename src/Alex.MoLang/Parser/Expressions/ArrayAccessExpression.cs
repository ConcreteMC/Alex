using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;

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
			var name = Array is NameExpression expression ? expression.Name.Path.ToString() :
				Array.Evaluate(scope, environment).AsString();

			var path = new MoPath($"{name}.{(int)Index.Evaluate(scope, environment).AsDouble()}");

			return environment.GetValue(path);
		}

		/// <inheritdoc />
		public override void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			var name = Array is NameExpression expression ? expression.Name.Path.ToString() :
				Array.Evaluate(scope, environment).AsString();

			var path = new MoPath($"{name}.{(int)Index.Evaluate(scope, environment).AsDouble()}");

			environment.SetValue(path, value);
		}
	}
}