using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Blocks.Properties;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json.BlockStates;
using NLog;

namespace Alex.Blocks.State
{
	public class BlockState : IEquatable<BlockState>, IRegistryEntry<BlockState>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockState));

		protected internal Dictionary<string, string> Values { get; set; }

		public BlockState()
		{
			Values = new Dictionary<string, string>(/*new StateComparer()*/);
		}

		public string Name { get; set; }
		public uint ID { get; set; }
		public BlockModel Model { get; set; }
		public Block Block { get; set; } = new Air();
		public bool IsMultiPart { get; set; } = false;
		public bool Default { get; set; } = false;

		public string[] AppliedModels { get; set; } = new string[0];
		
		//TODO: Remove
		internal BlockStateResource MultiPartHelper { get; set; }
		
		public BlockStateVariantMapper VariantMapper { get; set; }

		public T GetTypedValue<T>(IStateProperty<T> property)
		{
			if (Values.TryGetValue(property.Name, out var value))
			{
				return property.ParseValue(value);
			}

			return default(T);
		}

		public BlockState WithPropertyNoResolve(string property, string value, bool clone = true)
		{
			BlockState cloned;
			if (clone)
			{
				cloned = Clone();
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

		public BlockState WithProperty(string property, string value, bool prioritize, params string[] requiredMatches)
		{
			if (VariantMapper.TryResolve(this, property, value, prioritize, out BlockState result, requiredMatches))
			{
				return result;
			}

			return WithPropertyNoResolve(property, value);
		}
		
		public BlockState WithProperty(string property, string value)
		{
			return WithProperty(property, value, true);
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
		
		public bool TryGetValue(string property, out string value)
		{
			return Values.TryGetValue(property, out value);
			
			var r = Values.FirstOrDefault(x => x.Key.Equals(property, StringComparison.InvariantCultureIgnoreCase));
			if (!r.Equals(default(KeyValuePair<string, string>)))
			{
				value = r.Value.ToString().ToLowerInvariant();
				return true;
			}

			value = default(string);
			return false;
		}

		public bool ExactMatch(BlockState o)
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
			return ID == other.ID;
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

		public BlockState CloneSilent()
		{
			BlockState bs = new BlockState
			{
				Name = Name,
				ID = ID,
				Values = new Dictionary<string, string>(Values /*, new StateComparer()*/),
				Block = Block,
				VariantMapper = VariantMapper,
				Model = Model,
				Default = Default,
			//	Location = Location == null ? null : new ResourceLocation(Location.Namespace, Location.Path),
				AppliedModels = AppliedModels,
				IsMultiPart = IsMultiPart,
				MultiPartHelper = MultiPartHelper
			};
			return bs;
		}

		public BlockState Clone()
		{
			return CloneSilent();
		}

		public ResourceLocation Location { get; private set; }
		public IRegistryEntry<BlockState> WithLocation(ResourceLocation location)
		{
			Location = location;

			return this;
		}

		public BlockState Value => this;
	}
}
