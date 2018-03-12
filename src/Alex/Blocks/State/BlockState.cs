using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Blocks.Properties;
using Alex.API.Blocks.State;
using Alex.API.World;

namespace Alex.Blocks.State {

	public abstract class StateProperty : IStateProperty
	{
		public static Dictionary<string, StateProperty> _registeredTypes = new Dictionary<string, StateProperty>(StringComparer.InvariantCultureIgnoreCase);
		public string Name { get; }
		public Type PropertyType { get; }

		protected StateProperty(string name, Type propertyType)
		{
			Name = name;
			PropertyType = propertyType;

			_registeredTypes.TryAdd(name, this);
		}

		public abstract object ValueFromString(string value);
		public abstract object[] GetValidValues();

		public static StateProperty Parse(string name)
		{
			if (_registeredTypes.TryGetValue(name, out StateProperty property))
			{
				return property;
			}

			return new UnknownProperty(name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((StateProperty) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0) * 397) /*^ (PropertyType != null ? PropertyType.GetHashCode() : 0)*/;
			}
		}

		public bool Equals(IStateProperty other)
		{
			return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) ;/*&&
			       other.PropertyType.IsEquivalentTo(PropertyType);*/
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

		public override object[] GetValidValues()
		{
			return new object[0];
		}
	}

	public abstract class StateProperty<TType> : StateProperty, IStateProperty<TType>
	{
		protected StateProperty(string name) : base(name, typeof(TType))
		{

		}

		public abstract TType ParseValue(string value);
		public abstract string ToString(TType v);

		public override object ValueFromString(string value)
		{
			return ParseValue(value);
		}
	}

	public sealed class BlockState : IBlockState, IEquatable<BlockState>
	{
		private Dictionary<IStateProperty, object> _values { get; set; }

		public BlockState()
		{
			_values = new Dictionary<IStateProperty, object>(new EqualityCompare());
		}

		public string Name { get; set; }
		public uint ID { get; set; }

		public T GetTypedValue<T>(IStateProperty<T> property)
		{
			if (_values.TryGetValue(property, out var value))
			{
				if (property.PropertyType.IsEquivalentTo(value.GetType()))
				{
					return (T) value;
				}
				else
				{
					return property.ParseValue(value.ToString());
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

		public IBlockState WithProperty<T>(IStateProperty<T> property, T value)
		{
			if (_values.ContainsKey(property))
			{
				_values[property] = value;
			}
			else
			{
				_values.TryAdd(property, value);
			}

			return this;
		}

		public IBlockState WithProperty(IStateProperty property, string value)
		{
			return WithProperty(property, property.ValueFromString(value));
		}

		public IDictionary<IStateProperty, string> ToDictionary()
		{
			var dictionary = new Dictionary<IStateProperty, string>();
			foreach (var kv in _values)
			{
				dictionary.TryAdd(kv.Key, kv.Value.ToString().ToLowerInvariant());
			}
			return dictionary;
		}

		public bool TryGetValue(IStateProperty property, out object value)
		{
			return _values.TryGetValue(property, out value);
		}

		public bool TryGetValue(string property, out string value)
		{
			var r = _values.FirstOrDefault(x => x.Key.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));
			if (!r.Equals(default(KeyValuePair<IStateProperty, object>)))
			{
				value = r.Value.ToString().ToLowerInvariant();
				return true;
			}

			value = default(string);
			return false;
		}

		public IBlock Block = new Air();
		public IBlock GetBlock()
		{
			return Block;
		}

		public bool Equals(BlockState other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Block.Equals(other.Block) && _values.SequenceEqual(other._values) && string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase) && ID == other.ID;
		}

		public bool Equals(IBlockState other)
		{
			return Equals((BlockState)other);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is BlockState && Equals((BlockState)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hc = 0;
			//	if (Paths != null)
					foreach (var p in _values)
					{
						hc ^= p.GetHashCode();
						hc = (hc << 7) | (hc >> (32 - 7)); //rotale hc to the left to swipe over all bits
					}
				

				int hashCode = 13;
				hashCode = (hashCode * 397) ^ (int)ID;
				hashCode = (hashCode * 397) ^ hc;


				return (int)hashCode;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			var v = _values.ToArray();
			for (var index = 0; index < v.Length; index++)
			{
				var kv = v[index];

				sb.Append(kv.Key.Name);
				sb.Append('=');
				sb.Append(kv.Value.ToString().ToLowerInvariant());

				if (index != v.Length -1)
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
				var parsed = StateProperty.Parse(kv.Key);
				state._values.TryAdd(parsed, parsed.ValueFromString(kv.Value));
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

		internal static int GetVarIntSize(uint value)
		{
			int c = 0;
			do
			{
				value >>= 7;
				c++;
			} while (value != 0);

			return c;
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

		public IBlockState Clone()
		{
			BlockState bs = new BlockState();
			bs.Name = Name;
			bs.ID = ID;
			bs._values = new Dictionary<IStateProperty, object>(_values.ToArray());
			return bs;
		}

		private class EqualityCompare : IEqualityComparer<IStateProperty>
		{
			public bool Equals(IStateProperty x, IStateProperty y)
			{
				return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
			}

			public int GetHashCode(IStateProperty obj)
			{
				return obj.Name.GetHashCode();
			}
		}
	}
}
