using System;
using System.Collections.Generic;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;

namespace Alex.MoLang.Runtime.Struct
{
	public class ArrayStruct : VariableStruct
	{
		public ArrayStruct() { }

		public ArrayStruct(IEnumerable<IMoValue> values)
		{
			int index = 0;

			foreach (var value in values)
			{
				Map.Add(index.ToString(), value);
				index++;
			}
		}

		public ArrayStruct(IEnumerable<KeyValuePair<string, IMoValue>> map) : base(map) { }

		/// <inheritdoc />
		public override void Set(MoPath key, IMoValue value)
		{
			//string[] parts = key.ToString().Split(".");
			//parts[^1] = int.Parse(parts[^1]).ToString();

			base.Set(key, value);
		}
	}
}