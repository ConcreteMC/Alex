using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Alex.MoLang.Attributes;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Value;
using NLog;

namespace Alex.MoLang.Runtime.Struct
{
	public abstract class ValueAccessor
	{
		public abstract IMoValue Get(object instance);
		
		public abstract void Set(object instance, IMoValue value);
	}

	public class PropertyAccessor : ValueAccessor
	{
		private PropertyInfo _propertyInfo;
		public PropertyAccessor(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}
		
		/// <inheritdoc />
		public override IMoValue Get(object instance)
		{
			var value = _propertyInfo.GetValue(instance);

			return value is IMoValue moValue ? moValue : MoValue.FromObject(value);
			return (IMoValue) _propertyInfo.GetValue(instance);
		}

		/// <inheritdoc />
		public override void Set(object instance, IMoValue value)
		{
			if (!_propertyInfo.CanWrite)
				return;
			
			_propertyInfo.SetValue(instance, value);
		}
	}
	
	public class FieldAccessor : ValueAccessor
	{
		private FieldInfo _propertyInfo;
		public FieldAccessor(FieldInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}
		
		/// <inheritdoc />
		public override IMoValue Get(object instance)
		{
			var value = _propertyInfo.GetValue(instance);

			return value is IMoValue moValue ? moValue : MoValue.FromObject(value);
		}

		/// <inheritdoc />
		public override void Set(object instance, IMoValue value)
		{
			_propertyInfo.SetValue(instance, value);
		}
	}

	public class PropertyCache
	{
		public readonly Dictionary<string, ValueAccessor> Properties = new(StringComparer.OrdinalIgnoreCase);
		public readonly Dictionary<string, Func<object, MoParams, IMoValue>> Functions = new(StringComparer.OrdinalIgnoreCase);

		public PropertyCache(Type arg)
		{
			BuildFunctions(arg, Functions, Properties);
		}
		
		private static void BuildFunctions(Type type, Dictionary<string, Func<object, MoParams, IMoValue>> funcs, Dictionary<string, ValueAccessor> valueAccessors)
		{
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

			foreach (var method in methods)
			{
				var functionAttribute = method.GetCustomAttribute<MoFunctionAttribute>();
				if (functionAttribute == null)
					continue;

				foreach (var name in functionAttribute.Name)
				{
					if (funcs.ContainsKey(name))
						continue;

					var methodParams = method.GetParameters();

					IMoValue executeMolangFunction(object instance, MoParams mo)
					{
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

					funcs.Add(
						name, executeMolangFunction);
				}
			}

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
	
	public class ObjectStruct : IMoStruct
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ObjectStruct));
		private object _instance;
		private readonly Dictionary<string, ValueAccessor> _properties;// = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, Func<object, MoParams, IMoValue>> _functions;// = new(StringComparer.OrdinalIgnoreCase);

		private static readonly ConcurrentDictionary<Type, PropertyCache> _propertyCaches =
			new ConcurrentDictionary<Type, PropertyCache>();
		
		public ObjectStruct(object instance)
		{
			_instance = instance;
			
			var type = instance.GetType();

			var propCache = _propertyCaches.GetOrAdd(type, t => new PropertyCache(t));
			_properties = propCache.Properties;
			_functions = propCache.Functions;
		}
		
		/// <inheritdoc />
		public object Value => _instance;

		/// <inheritdoc />
		public void Set(string key, IMoValue value)
		{
			var index = key.IndexOf('.');

			if (index < 0)
			{
				if (_properties.TryGetValue(key, out var accessor)) {
					//Map.TryAdd(main, container = new VariableStruct());
					accessor.Set(_instance, value);
					return;
				}
				
				throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
			}

			string main = key.Substring(0, index);

			if (!string.IsNullOrWhiteSpace(main)) {
				//object vstruct = Get(main, MoParams.Empty);

				if (!_properties.TryGetValue(main, out var accessor)) {
					//Map.TryAdd(main, container = new VariableStruct());
					throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
				}

				var container = accessor.Get(_instance);
				
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
		public IMoValue Get(string key, MoParams parameters)
		{
			var index = key.IndexOf('.');

			if (index >= 0)
			{
				string main = key.Substring(0, index);

				if (!string.IsNullOrWhiteSpace(main))
				{
					IMoValue value = null; //Map[main];

					if (_properties.TryGetValue(main, out var accessor))
					{
						value = accessor.Get(_instance);
					}
					else if (_functions.TryGetValue(main, out var func))
					{
						value = func.Invoke(_instance, parameters);
					}
					else
					{
						return DoubleValue.Zero;
					}

					if (value is IMoStruct moStruct)
					{
						return moStruct.Get(key.Substring(index + 1), parameters);
					}

					return value;
				}
			}

			if (_properties.TryGetValue(key, out var v))
				return v.Get(_instance);
			
			if (_functions.TryGetValue(key, out var f))
				return f.Invoke(_instance, parameters);
			
			Log.Debug($"({_instance.GetType().Name}) Unknown query: {key}");
			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public void Clear()
		{
			throw new NotImplementedException();
		}
	}
}