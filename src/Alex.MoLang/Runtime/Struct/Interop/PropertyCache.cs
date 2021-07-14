using System;
using System.Collections.Generic;
using System.Reflection;
using Alex.MoLang.Attributes;
using Alex.MoLang.Runtime.Value;
using NLog;

namespace Alex.MoLang.Runtime.Struct
{
	public class PropertyCache
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PropertyCache));
		
		public readonly Dictionary<string, ValueAccessor> Properties = new(StringComparer.OrdinalIgnoreCase);
		public readonly Dictionary<string, Func<object, MoParams, IMoValue>> Functions = new(StringComparer.OrdinalIgnoreCase);

		public PropertyCache(Type arg)
		{
			ProcessMethods(arg, Functions);
			ProcessProperties(arg, Properties);
		}
		
		private static void ProcessMethods(IReflect type, IDictionary<string, Func<object, MoParams, IMoValue>> functions)
		{
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

			foreach (var method in methods)
			{
				var functionAttribute = method.GetCustomAttribute<MoFunctionAttribute>();
				if (functionAttribute == null)
					continue;

				foreach (var name in functionAttribute.Name)
				{
					if (functions.ContainsKey(name))
					{
						Log.Warn($"Duplicate function \'{name}\' in {type.ToString()}");
						continue;
					}

					IMoValue ExecuteMolangFunction(object instance, MoParams mo)
					{
						var methodParams = method.GetParameters();
						IMoValue value = DoubleValue.Zero;

						object[] parameters = new object[methodParams.Length];
						//List<object> parameters = new List<object>();

						if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof(MoParams))
						{
							parameters[0] = mo;
							//parameters.Add(mo);
						}
						else
						{
							for (var index = 0; index < methodParams.Length; index++)
							{
								var parameter = methodParams[index];

								if (!mo.Contains(index))
								{
									if (!parameter.IsOptional) throw new MissingMethodException($"Missing parameter: {parameter.Name}");

									break;
								}

								var t = parameter.ParameterType;

								if (t == typeof(MoParams))
								{
									parameters[index] = mo; //.Add(mo);
								}
								else if (t == typeof(int))
								{
									parameters[index] = mo.GetInt(index);
								}
								else if (t == typeof(double))
								{
									parameters[index] = mo.GetDouble(index);
								}
								else if (t == typeof(float))
								{
									parameters[index] = (float) mo.GetDouble(index);
								}
								else if (t == typeof(string))
								{
									parameters[index] = mo.GetString(index);
								}
								else if (typeof(IMoStruct).IsAssignableFrom(t))
								{
									parameters[index] = mo.GetStruct(index);
								}
								else if (typeof(MoLangEnvironment).IsAssignableFrom(t))
								{
									parameters[index] = mo.GetEnv(index);
								}
								else
								{
									throw new Exception("Unknown parameter type.");
								}

								//TODO: Continue.
							}
						}

						var result = method.Invoke(instance, parameters);

						if (result != null)
						{
							if (result is IMoValue moValue) return moValue;

							return MoValue.FromObject(result);
						}

						return value;
					}

					functions.Add(
						name, ExecuteMolangFunction);
				}
			}
		}
		
		private static void ProcessProperties(IReflect type, IDictionary<string, ValueAccessor> valueAccessors)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in properties)
			{
				foreach (var functionAttribute in prop.GetCustomAttributes<MoPropertyAttribute>())
				{
					if (valueAccessors.ContainsKey(functionAttribute.Name))
						continue;

					valueAccessors.Add(functionAttribute.Name, new PropertyAccessor(prop));
				}
			}
			
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in fields)
			{
				foreach (var functionAttribute in prop.GetCustomAttributes<MoPropertyAttribute>())
				{
					if (valueAccessors.ContainsKey(functionAttribute.Name))
						continue;

					valueAccessors.Add(functionAttribute.Name, new FieldAccessor(prop));
				}
			}
		}
	}
}