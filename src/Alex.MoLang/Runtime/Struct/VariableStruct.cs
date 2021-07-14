using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;
using NLog;

namespace Alex.MoLang.Runtime.Struct
{
	public class VariableStruct : IMoStruct
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(VariableStruct));
		public readonly Dictionary<string, IMoValue> Map;

		/// <inheritdoc />
		public object Value => Map;

		public VariableStruct()
		{
			Map = new(StringComparer.OrdinalIgnoreCase);
		}

		public VariableStruct(IEnumerable<KeyValuePair<string, IMoValue>> values)
		{
			if (values != null)
			{
				Map = new Dictionary<string, IMoValue>(values, StringComparer.OrdinalIgnoreCase);
			}
		}

		/// <inheritdoc />
		public virtual void Set(MoPath key, IMoValue value)
		{
			//var index = key.IndexOf('.');

			if (!key.HasChildren)
			{
				Map[key.ToString()] = value;

				return;
			}

			string main = key.Segment;

			if (!string.IsNullOrWhiteSpace(main)) {
				//object vstruct = Get(main, MoParams.Empty);

				if (!Map.TryGetValue(main, out var container)) {
					Map.TryAdd(main, container = new VariableStruct());
					//	throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
				}
				
				if (container is IMoStruct moStruct)
				{
					moStruct.Set(key.Segments[0], value);
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
		public virtual IMoValue Get(MoPath key, MoParams parameters)
		{
			//var index = key.IndexOf('.');

			if (key.HasChildren)
			{
				string main = key.Segment;

				if (!string.IsNullOrWhiteSpace(main))
				{
					IMoValue value = null; //Map[main];

					if (!Map.TryGetValue(main, out value))
					{
						//		Log.Info($"Unknown variable map: {key}");
						return DoubleValue.Zero;
					}

					if (value is IMoStruct moStruct)
					{
						return moStruct.Get(key.Segments[0], parameters);
					}

					return value;
				}
			}

			if (Map.TryGetValue(key.ToString(), out var v))
				return v;
			
			//
		//	Log.Info($"Unknown variable: {key}");
			
			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Map.Clear();
		}
	}
}