using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Blocks.Properties;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib.Json.BlockStates;
using NLog;

namespace Alex.Blocks.State
{
	public sealed class BlockStateVariantMapper
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockStateVariantMapper));
		
		internal IBlockState _default;
		private IList<IBlockState> Variants { get; } = new List<IBlockState>();

		public BlockStateVariantMapper()
		{

		}
		
		public bool TryResolve(BlockState source, string property, string value, bool prioritize, out IBlockState result, params string[] requiredMatches)
		{
			var copiedProperties = source.ToDictionary();
			copiedProperties[property] = value.ToString();

			int highestMatch = 0;
			IBlockState highest = null;

			foreach (var variant in Variants.ToArray().Where(x => (x.TryGetValue(property, out string xVal) && xVal.Equals(value, StringComparison.InvariantCultureIgnoreCase))))
			{
				bool valid = true;
				foreach (var requiredMatch in requiredMatches)
				{
					if (!(copiedProperties.TryGetValue(requiredMatch, out string copyValue) && variant.TryGetValue(requiredMatch, out string variantValue) && copyValue == variantValue))
					{
						valid = false;
						break;
					}
				}
				
				if (!valid)
					continue;
				
				int matches = 0;
				foreach (var copy in copiedProperties.Where(x => x.Key != property))
				{
					//Check if variant value matches copy value.
					if (variant.TryGetValue(copy.Key, out string val) && copy.Value.Equals(val, StringComparison.InvariantCultureIgnoreCase))
					{
						matches++;
					}
				}

				foreach (var variantProp in variant.ToDictionary())
				{
					if (!copiedProperties.ContainsKey(variantProp.Key))
					{
						matches--;
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

		protected internal Dictionary<string, string> Values { get; set; }

		public BlockState()
		{
			Values = new Dictionary<string, string>(/*new StateComparer()*/);
		}

		public string Name { get; set; }
		public uint ID { get; set; }
		public IBlockModel Model { get; set; }
		public IBlock Block { get; set; } = new Air();
		public bool IsMultiPart { get; set; } = false;

		//TODO: Remove
		internal BlockStateResource MultiPartHelper { get; set; }
		
		public BlockStateVariantMapper VariantMapper { get; set; }

		public T GetTypedValue<T>(IStateProperty<T> property)
		{
			if (Values.TryGetValue(property.Name, out var value))
			{
				/*if (property.PropertyType.IsEquivalentTo(value.GetType()))
				{
					return (T)value;
				}
				else
				{*/
					return property.ParseValue(value);
				//}
			}

			return property.GetDefaultValue();
		}

		public object GetValue(string property)
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

		public IBlockState WithPropertyNoResolve(string property, string value, bool clone = true)
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
			//return WithPropertyNoResolve(property, property.ValueFromString(value), clone);
		}

		/*public IBlockState WithPropertyNoResolve(IStateProperty property, object value, bool clone = true)
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
		}*/

		/*public IBlockState WithProperty(IStateProperty property, object value)
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
		}*/

		public IBlockState WithProperty(string property, string value, bool prioritize, params string[] requiredMatches)
		{
			if (VariantMapper.TryResolve(this, property, value, prioritize, out IBlockState result, requiredMatches))
			{
				return result;
			}

			return WithPropertyNoResolve(property, value);
			//return WithProperty(property, property.ValueFromString(value));
		}

		public IDictionary<string, string> ToDictionary()
		{
			var dictionary = new Dictionary<string, string>(/*new StateComparer()*/);
			foreach (var kv in Values)
			{
				dictionary.TryAdd(kv.Key, kv.Value.ToString().ToLowerInvariant());
			}
			return dictionary;
		}

		/*public bool TryGetValue(IStateProperty property, out object value)
		{
			return Values.TryGetValue(property, out value);
		}*/

		public bool TryGetValue(string property, out string value)
		{
			var r = Values.FirstOrDefault(x => x.Key.Equals(property, StringComparison.InvariantCultureIgnoreCase));
			if (!r.Equals(default(KeyValuePair<string, string>)))
			{
				value = r.Value.ToString().ToLowerInvariant();
				return true;
			}

			value = default(string);
			return false;
		}

		public bool ExactMatch(IBlockState o)
		{
			if (o is BlockState other)
			{
				if (Values.Count != other.Values.Count)
				{
					return false;
				}

				bool equal = true;
				foreach (var pair in ToDictionary())
				{
					// value;
					if (other.TryGetValue(pair.Key, out string value))
					{
						// Require value be equal.
						if (!value.Equals(pair.Value))
						{
							equal = false;
							break;
						}
					}
					else
					{
						// Require key be present.
						equal = false;
						break;
					}
				}

				return equal;
			}
			else
			{
				return false;
			}
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

				sb.Append(kv.Key);
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
				//var parsed = StateProperty.Parse(kv.Key);
				//state.Values.TryAdd(parsed, kv.Value);
				state.Values.Add(kv.Key, kv.Value);
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
			bs.Values = new Dictionary<string, string>(Values/*, new StateComparer()*/);
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
