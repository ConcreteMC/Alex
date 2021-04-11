using System;

namespace Alex.MoLang.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class MoFunctionAttribute : Attribute
	{
		public string Name { get; }
		public MoFunctionAttribute(string functionName)
		{
			Name = functionName;
		}
	}
}