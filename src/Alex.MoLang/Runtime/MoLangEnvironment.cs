using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;

namespace Alex.MoLang.Runtime
{
	public class MoLangEnvironment : IMoValue
	{
		private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(MoLangEnvironment));
		/// <inheritdoc />
		public object Value => Structs;

		public Dictionary<string, IMoStruct> Structs { get; } = new(StringComparer.OrdinalIgnoreCase);
		public IMoValue ThisVariable { get; set; } = DoubleValue.Zero;

		public MoLangEnvironment()
		{
			Structs.TryAdd("math", MoLangMath.Library);
			Structs.TryAdd("temp", new VariableStruct());
			Structs.TryAdd("variable", new VariableStruct());
			Structs.TryAdd("array", new ArrayStruct());

			Structs.TryAdd("context", new ContextStruct());
		}
		
		public IMoValue GetValue(MoPath name) {
			return GetValue(name, MoParams.Empty);
		}

		public IMoValue GetValue(MoPath name, MoParams param) {
			try
			{
		//		var index = name.IndexOf('.');
				//string[] segments = name.;
			//	string main = name.Substring(0, index); //.Dequeue();

				//if (!Structs.ContainsKey(main))
				//{
				//	throw new MoLangRuntimeException($"Cannot retrieve struct: {name}", null);
				//}
				return Structs[name.Value].Get(name.Next, param);
			}
			catch (Exception ex)
			{
				throw new MoLangRuntimeException($"Cannot retrieve struct: {name}", ex);
			}
		}

		public void SetValue(MoPath name, IMoValue value)
		{
			try
			{
				//var index = name.IndexOf('.');
				//string main = name.Substring(0, index);

				//if (!Structs.ContainsKey(main)) {
				//	throw new MoLangRuntimeException($"Cannot set value on struct: {name}", null);
				//}

				Structs[name.Value].Set(name.Next, value);
			}
			catch (Exception ex)
			{
				throw new MoLangRuntimeException($"Cannot set value on struct: {name}", ex);
			}
		}
		
		/// <inheritdoc />
		public bool Equals(IMoValue b)
		{
			return Equals((object)b);
		}
	}
}