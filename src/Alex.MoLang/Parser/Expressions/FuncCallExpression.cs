using System;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class FuncCallExpression : Expression<IExpression>
	{
		public IExpression   Name;
		public IExpression[] Args;

		public FuncCallExpression(IExpression name, IExpression[] args) : base(null)
		{
			Name = name;
			Args = args;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			List<IExpression> p = Args.ToList();
			string name = Name is NameExpression ? ((NameExpression) Name).Value : Name.Evaluate(scope, environment).ToString();

			return environment.GetValue(name, new MoParams(
				p.Select(x => x.Evaluate(scope, environment)).ToList()
			));
		}
	}
}