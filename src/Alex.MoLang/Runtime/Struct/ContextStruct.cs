using System;
using System.Collections.Generic;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public class ContextStruct : VariableStruct
	{
		public ContextStruct()
		{
			
		}
		
		public ContextStruct(IEnumerable<KeyValuePair<string, IMoValue>> values) : base(values)
		{
			
		}
		
		/// <inheritdoc />
		public override void Set(string key, IMoValue value)
		{
			throw new NotSupportedException("Read-only context");
		}
	}
}