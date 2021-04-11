using System;
using System.Reflection;
using Alex.MoLang.Attributes;

namespace Alex.MoLang.Runtime.Struct
{
	public class ObjectStruct<T> : QueryStruct
	{
		private T _instance;
		public ObjectStruct(T instance)
		{
			_instance = instance;
			BuildFunctions(typeof(T));
		}

		private void BuildFunctions(Type type)
		{
			var methods = type.GetMethods(BindingFlags.Public);

			foreach (var method in methods)
			{
				var functionAttribute = method.GetCustomAttribute<FunctionAttribute>();
				if (functionAttribute == null)
					continue;

				if (Functions.ContainsKey(method.Name))
					continue;

				foreach (var parameter in method.GetParameters())
				{
					//TODO: Continue.
				}
			}
		}
	}
}