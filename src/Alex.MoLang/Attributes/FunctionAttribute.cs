using System;

namespace Alex.MoLang.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class FunctionAttribute : Attribute
	{
		public string Name { get; }
		public FunctionAttribute(string functionName)
		{
			Name = functionName;
		}
	}
}