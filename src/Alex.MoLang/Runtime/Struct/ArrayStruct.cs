using System.Collections.Generic;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public class ArrayStruct : VariableStruct
	{
		public ArrayStruct() {}

		public ArrayStruct(IEnumerable<KeyValuePair<string, IMoValue>> map) : base(map)
		{
			
		}

		/// <inheritdoc />
		public override void Set(string key, IMoValue value)
		{
			string[] parts = key.Split(".");
			parts[^1] = int.Parse(parts[^1]).ToString();
			
			base.Set(string.Join(".", parts), value);
		}
	}
}