using System;
using System.Collections.Generic;
using Alex.API.World;

namespace Alex.Blocks.State {

	public abstract class StateProperty
	{
		private static Dictionary<string, StateProperty> _registeredTypes = new Dictionary<string, StateProperty>();
		public string Name { get; }
		public Type PropertyType { get; }

		protected StateProperty(string name, Type propertyType)
		{
			Name = name;
			PropertyType = propertyType;

			_registeredTypes.TryAdd(name, this);
		}

		protected abstract object ValueFromString(string value);

		public static StateProperty Parse(string name)
		{
			if (_registeredTypes.TryGetValue(name, out StateProperty property))
			{
				return property;
			}

			return new UnknownProperty(name);
		}
	}

	public class UnknownProperty : StateProperty
	{
		public UnknownProperty(string name) : base(name, typeof(string))
		{
		}

		protected override object ValueFromString(string value)
		{
			return value;
		}
	}

	public abstract class StateProperty<TType> : StateProperty
	{
		protected StateProperty(string name) : base(name, typeof(TType))
		{

		}

		public abstract TType ParseValue(string value);

		protected override object ValueFromString(string value)
		{
			return ParseValue(value);
		}
	}

	public class BlockState
	{
		private Dictionary<StateProperty, object> _values { get; }
		public BlockState()
		{
			_values = new Dictionary<StateProperty, object>();
		}

		public T GetValue<T>(StateProperty<T> property)
		{
			if (_values.TryGetValue(property, out var value))
			{
				if (property.PropertyType.IsEquivalentTo(value.GetType()))
				{
					return (T) value;
				}
			}

			return default(T);
		}

		public BlockState WithProperty<T>(StateProperty<T> property, T value)
		{
			if (!_values.TryAdd(property, value))
			{
				_values[property] = value;
			}
			return this;
		}

		public Dictionary<StateProperty, string> ToDictionary()
		{
			var dictionary = new Dictionary<StateProperty, string>();
			foreach (var kv in _values)
			{
				dictionary.TryAdd(kv.Key, kv.Value.ToString());
			}
			return dictionary;
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public static BlockState FromString(string data)
		{
			BlockState state = new BlockState();

			var kvs = ParseData(data);
			foreach (var kv in kvs)
			{
				state._values.TryAdd(StateProperty.Parse(kv.Key), kv.Value);
			}

			return state;
		}

		private static Dictionary<string, string> ParseData(string variant)
		{
			Dictionary<string, string> values = new Dictionary<string, string>();

			string[] splitVariants = variant.Split(',');
			foreach (var split in splitVariants)
			{
				string[] splitted = split.Split('=');
				if (splitted.Length <= 1)
				{
					continue;
				}

				string key = splitted[0];
				string value = splitted[1];

				values.Add(key, value);
			}

			return values;
		}
	}
}
