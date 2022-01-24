using System;

namespace Alex.MoLang.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class MoFunctionAttribute : Attribute
	{
		public string[] Name { get; }

		public MoFunctionAttribute(params string[] functionNames)
		{
			Name = functionNames;
		}
	}
}