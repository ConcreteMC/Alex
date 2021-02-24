using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public class VariableStruct : IMoStruct
	{
		public readonly Dictionary<string, IMoValue> Map = new Dictionary<string, IMoValue>();

		/// <inheritdoc />
		public object Value => Map;

		public VariableStruct()
		{
			
		}

		public VariableStruct(IEnumerable<KeyValuePair<string, IMoValue>> values)
		{
			Map = new Dictionary<string, IMoValue>(values);
		}

		/// <inheritdoc />
		public virtual void Set(string key, IMoValue value)
		{
			Queue<string> segments = new Queue<string>(key.Split("."));
			string        main     = segments.Dequeue();

			if (segments.Count > 0 && main != null) {
				object vstruct = Get(main, MoParams.Empty);

				if (!(vstruct is IMoStruct)) {
					vstruct = new VariableStruct();
				}
				
				((IMoStruct) vstruct).Set(segments.Dequeue(), value);

				Map[main] = (IMoStruct)vstruct;//.Add(main, (IMoStruct) vstruct);
			} else
			{
				Map[key] = value;
				//Map.Add(key, value);
			}
		}

		/// <inheritdoc />
		public virtual IMoValue Get(string key, MoParams parameters)
		{
			Queue<string> segments = new Queue<string>(key.Split("."));
			var           main     = segments.Dequeue();
			
			if (segments.Count > 0 && main != null)
			{
				var vstruct = Map[main];

				if (vstruct is IMoStruct)
				{
					return ((IMoStruct) vstruct).Get(segments.Dequeue(), MoParams.Empty);
				}
			}

			if (Map.TryGetValue(key, out var v))
				return v;

			return null;
			return Map[key];
		}

		/// <inheritdoc />
		public void Clear()
		{
			Map.Clear();
		}
	}
}