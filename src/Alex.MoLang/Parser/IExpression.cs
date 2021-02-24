using System;
using System.Collections.Generic;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser
{
	public interface IExpression
	{
		Dictionary<string, object> Attributes { get; }
		
		IMoValue Evaluate(MoScope scope, MoLangEnvironment environment);

		void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value);
	}

	public abstract class Expression<T> : IExpression
	{
		protected Expression(T value)
		{
			Value = value;
		}

		public T Value { get; protected set; }
		
		/// <inheritdoc />
		public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

		/// <inheritdoc />
		public abstract IMoValue Evaluate(MoScope scope, MoLangEnvironment environment);

		/// <inheritdoc />
		public virtual void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			throw new Exception("Cannot assign a value to " + this.GetType());
		}
	}
}