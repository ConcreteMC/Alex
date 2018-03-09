using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Blocks.Properties;
using Alex.API.Blocks.State;
using Alex.API.World;

namespace Alex.Blocks.State {

	public abstract class StateProperty : IStateProperty
	{
		public static Dictionary<string, StateProperty> _registeredTypes = new Dictionary<string, StateProperty>();
		public string Name { get; }
		public Type PropertyType { get; }

		protected StateProperty(string name, Type propertyType)
		{
			Name = name;
			PropertyType = propertyType;

			_registeredTypes.TryAdd(name, this);
		}

		public abstract object ValueFromString(string value);

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

		public override object ValueFromString(string value)
		{
			return value;
		}
	}

	public abstract class StateProperty<TType> : StateProperty, IStateProperty<TType>
	{
		protected StateProperty(string name) : base(name, typeof(TType))
		{

		}

		public abstract TType ParseValue(string value);

		public override object ValueFromString(string value)
		{
			return ParseValue(value);
		}
	}

	public sealed class BlockState : IBlockState
	{
		private Dictionary<IStateProperty, object> _values { get; }
		public BlockState()
		{
			_values = new Dictionary<IStateProperty, object>();
		}

		public T GetTypedValue<T>(IStateProperty<T> property)
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

		public object GetValue(IStateProperty property)
		{
			if (_values.TryGetValue(property, out var value))
			{
				if (property.PropertyType.IsEquivalentTo(value.GetType()))
				{
					return value;
				}
			}

			return null;
		}

		public IBlockState WithProperty(IStateProperty property, object value)
		{
			if (!_values.TryAdd(property, value))
			{
				_values[property] = value;
			}
			return this;
		}

		public IBlockState WithProperty(IStateProperty property, string value)
		{
			return WithProperty(property, property.ValueFromString(value));
		}

		public IDictionary<IStateProperty, object> ToDictionary()
		{
			var dictionary = new Dictionary<IStateProperty, object>();
			foreach (var kv in _values)
			{
				dictionary.TryAdd(kv.Key, kv.Value.ToString());
			}
			return dictionary;
		}

		private IBlock _block = new Air();
		public IBlock GetBlock()
		{
			return _block;
		}

		public void SetBlock(IBlock block)
		{
			_block = block;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			foreach (var kv in _values)
			{
				sb.Append(kv.Key.Name);
				sb.Append('=');
				sb.Append(kv.Value.ToString());
				sb.Append(',');
			}

			return sb.ToString();
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

		internal static int GetVarIntSize(int value)
		{
			int c = 0;
			do
			{
				value >>= 7;
				c++;
			} while (value != 0);

			return c;
		}
	}
}
