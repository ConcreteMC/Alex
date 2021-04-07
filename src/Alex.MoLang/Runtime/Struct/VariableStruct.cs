using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Value;
using NLog;

namespace Alex.MoLang.Runtime.Struct
{
	public class VariableStruct : IMoStruct
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(VariableStruct));
		public readonly Dictionary<string, IMoValue> Map = new(StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc />
		public object Value => Map;

		public VariableStruct()
		{
			
		}

		public VariableStruct(IEnumerable<KeyValuePair<string, IMoValue>> values)
		{
			if (values != null)
			{
				Map = new Dictionary<string, IMoValue>(values, StringComparer.OrdinalIgnoreCase);
			}
		}

		/// <inheritdoc />
		public virtual void Set(string key, IMoValue value)
		{
			var index = key.IndexOf('.');

			if (index < 0)
			{
				Map[key] = value;

				return;
			}

			string main = key.Substring(0, index);

			if (!string.IsNullOrWhiteSpace(main)) {
				//object vstruct = Get(main, MoParams.Empty);

				if (!Map.TryGetValue(main, out var container)) {
					Map.TryAdd(main, container = new VariableStruct());
					//	throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
				}
				
				if (container is IMoStruct moStruct)
				{
					moStruct.Set(key.Substring(index + 1), value);
				}
				else
				{
					throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
				}
				
				//((IMoStruct) vstruct).Set(string.Join(".", segments), value);

				//Map[main] = (IMoStruct)vstruct;//.Add(main, (IMoStruct) vstruct);
			}
		}

		/// <inheritdoc />
		public virtual IMoValue Get(string key, MoParams parameters)
		{
			var index = key.IndexOf('.');

			if (index >= 0)
			{
				string main = key.Substring(0, index);

				if (!string.IsNullOrWhiteSpace(main))
				{
					IMoValue value = null; //Map[main];

					if (!Map.TryGetValue(main, out value))
						return DoubleValue.Zero;

					if (value is IMoStruct moStruct)
					{
						return moStruct.Get(key.Substring(index + 1), parameters);
					}

					return value;
				}
			}

			if (Map.TryGetValue(key, out var v))
				return v;
			
			//Console.WriteLine($"Unknown variable: {key}");
			
			return DoubleValue.Zero;
			//
			Console.WriteLine($"Unknown variable: {key}");
			return DoubleValue.Zero;
			return Map[key];
		}

		/// <inheritdoc />
		public void Clear()
		{
			Map.Clear();
		}
	}
}