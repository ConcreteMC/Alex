using System;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class FuncCallExpression : Expression
	{
		public IExpression   Name { get; set; }
		public IExpression[] Args { get; set; }

		public FuncCallExpression(IExpression name, IExpression[] args)
		{
			Name = name;
			Args = args;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			//List<IExpression> p = Args.ToList();
			string name = Name is NameExpression expression ? expression.Name : Name.Evaluate(scope, environment).ToString();

			return environment.GetValue(name, new MoParams(
				Args.Select(x => x.Evaluate(scope, environment))
			));
		}
	}
}