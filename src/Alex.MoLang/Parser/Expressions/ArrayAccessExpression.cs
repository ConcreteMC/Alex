using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class ArrayAccessExpression : Expression<IExpression>
	{
		public IExpression Array;
		public IExpression Index;

		public ArrayAccessExpression(IExpression array, IExpression index) : base(null)
		{
			Array = array;
			Index = index;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			string name = Array is NameExpression ? ((NameExpression) Array).Value : Array.Evaluate(scope, environment).AsString();

			return environment.GetValue(name + "." + (int) Index.Evaluate(scope, environment).AsDouble());
		}

		/// <inheritdoc />
		public override void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			string name = Array is NameExpression ? ((NameExpression) Array).Value : Array.Evaluate(scope, environment).AsString();

			environment.SetValue(name + "." + (int) Index.Evaluate(scope, environment).AsDouble(), value);
		}
	}
}