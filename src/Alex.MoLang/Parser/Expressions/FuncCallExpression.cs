using System;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;

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
			MoPath name = Name is NameExpression expression ? expression.Name : new MoPath(Name.Evaluate(scope, environment).ToString());
			
			IMoValue[] arguments = new IMoValue[Args.Length];

			for (int i = 0; i < arguments.Length; i++)
			{
				arguments[i] = Args[i].Evaluate(scope, environment);
			}
			
			return environment.GetValue(name, new MoParams(arguments));
		}
	}
}