using System;

namespace Alex.MoLang.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class MoPropertyAttribute : Attribute
	{
		public string Name { get; }

		public MoPropertyAttribute(string name)
		{
			Name = name;
		}
	}
}