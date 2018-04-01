using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Blocks.Properties;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Utils;
using NLog;

namespace Alex.Blocks.State
{
	public sealed class BlockStateVariantMapper
	{
		internal IBlockState _default;
		private IList<IBlockState> Variants { get; } = new List<IBlockState>();

		public BlockStateVariantMapper()
		{

		}

		public bool TryResolve(BlockState copyFrom, IStateProperty property, object value, out IBlockState result)
		{
			var valuesCopied = new Dictionary<IStateProperty, object>(copyFrom.Values, new StateComparer());
			valuesCopied[property] = value;

			int highestMatch = 0;
			IBlockState highest = null;

			foreach (var variant in Variants.ToArray())
			{
				int matches = 0;
				foreach (var e in valuesCopied)
				{
					if (variant.TryGetValue(e.Key, out object val) && e.Value.Equals(val))
					{
						matches++;
					}
				}

				if (matches > highestMatch)
				{
					highestMatch = matches;
					highest = variant;
				}
			}

			if (highest != null)
			{
				result = highest;
				return true;
			}

			result = null;
			return false;
		}

		public bool TryAdd(IBlockState state)
		{
			//return Variants.TryAdd(state);
			if (Variants.Contains(state)) return false;
			Variants.Add(state);
			return true;
		}

		public IBlockState[] GetVariants()
		{
			return Variants.Concat(new []{ _default}).ToArray();
		}

		public IBlockState GetDefaultState()
		{
			return _default;
		}
	}

	public class BlockState : IBlockState, IEquatable<BlockState>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockState));

		protected internal Dictionary<IStateProperty, object> Values { get; set; }

		public BlockState()
		{
			Values = new Dictionary<IStateProperty, object>(new StateComparer());
		}

		public string Name { get; set; }
		public uint ID { get; set; }
		public IBlockModel Model { get; set; }
		public IBlock Block { get; set; } = new Air();
		public bool IsMultiPart { get; set; } = false;

		public BlockStateVariantMapper VariantMapper { get; set; }

		public T GetTypedValue<T>(IStateProperty<T> property)
		{
			if (Values.TryGetValue(property, out var value))
			{
				if (property.PropertyType.IsEquivalentTo(value.GetType()))
				{
					return (T)value;
				}
				else
				{
					return property.ParseValue(value.ToString());
				}
			}

			return property.GetDefaultValue();
		}

		public object GetValue(IStateProperty property)
		{
			if (Values.TryGetValue(property, out var value))
			{
				//if (property.PropertyType.IsEquivalentTo(value.GetType()))
				{
					return value;
				}
			}

			return null;
		}

		public IBlockState WithPropertyNoResolve(IStateProperty property, string value, bool clone = true)
		{
			return WithPropertyNoResolve(property, property.ValueFromString(value), clone);
		}

		public IBlockState WithPropertyNoResolve(IStateProperty property, object value, bool clone = true)
		{
			BlockState cloned;
			if (clone)
			{
				cloned = (BlockState)Clone();
			}
			else
			{
				cloned = this;
			}

			if (!cloned.Values.TryAdd(property, value))
			{
				cloned.Values[property] = value;
			}
			return cloned;
		}

		public IBlockState WithProperty(IStateProperty property, object value)
		{
			if (VariantMapper.TryResolve(this, property, value, out IBlockState result))
			{
				return result;
			}

			return WithPropertyNoResolve(property, value);
		}

		public IBlockState WithProperty<T>(IStateProperty<T> property, T value)
		{
			return WithProperty((IStateProperty)property, value);
		}

		public IBlockState WithProperty(IStateProperty property, string value)
		{
			return WithProperty(property, property.ValueFromString(value));
		}

		public IDictionary<IStateProperty, string> ToDictionary()
		{
			var dictionary = new Dictionary<IStateProperty, string>(new StateComparer());
			foreach (var kv in Values)
			{
				dictionary.TryAdd(kv.Key, kv.Value.ToString().ToLowerInvariant());
			}
			return dictionary;
		}

		public bool TryGetValue(IStateProperty property, out object value)
		{
			return Values.TryGetValue(property, out value);
		}

		public bool TryGetValue(string property, out string value)
		{
			var r = Values.FirstOrDefault(x => x.Key.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));
			if (!r.Equals(default(KeyValuePair<IStateProperty, object>)))
			{
				value = r.Value.ToString().ToLowerInvariant();
				return true;
			}

			value = default(string);
			return false;
		}

		public bool Equals(BlockState other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase) && ID == other.ID;
		}

		public bool Equals(IBlockState other)
		{
			return Equals((BlockState)other);
		}		

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			var v = Values.ToArray();
			for (var index = 0; index < v.Length; index++)
			{
				var kv = v[index];

				sb.Append(kv.Key.Name);
				sb.Append('=');
				sb.Append(kv.Value.ToString().ToLowerInvariant());

				if (index != v.Length - 1)
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
				state.Values.TryAdd(parsed, parsed.ValueFromString(kv.Value));
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

		public IBlockState CloneSilent()
		{
			BlockState bs = new BlockState();
			bs.Name = Name;
			bs.ID = ID;
			bs.Values = new Dictionary<IStateProperty, object>(Values, new StateComparer());
			bs.Block = Block;
			bs.VariantMapper = VariantMapper;
			bs.Model = Model;
			return bs;
		}

		public IBlockState Clone()
		{
			return CloneSilent();
		}
	}

	internal class StateComparer : IEqualityComparer<IStateProperty>
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
