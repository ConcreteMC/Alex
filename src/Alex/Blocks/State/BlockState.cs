using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alex.API.Blocks.Properties;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json.BlockStates;
using NLog;

namespace Alex.Blocks.State
{
	public class BlockState : IEquatable<BlockState>, IRegistryEntry<BlockState>, IEnumerable<KeyValuePair<string, string>>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockState));

		protected internal Dictionary<string, string> Values { get; set; }
		public             int                        Count  => Values.Count;
		public BlockState()
		{
			Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public string Name { get; set; }
		public uint ID { get; set; }

		private  BlockModel       _model = null;
		public BlockModel Model
		{
			get
			{
				return _model ?? new MissingBlockModel();
			}
			set
			{
				_model = value;
			}
		}

		public   Block            Block        { get; set; } = new Air();
		public   bool             IsMultiPart  { get; set; } = false;
		public   bool             Default      { get; set; } = false;

		//public string[] AppliedModels { get; set; } = new string[0];
		
		//TODO: Remove
		//internal BlockStateResource MultiPartHelper { get; set; }
		
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

			Log.Debug($"Invalid property on state {Name} ({property}={value})");
			return this;
		}
		
		public BlockState WithProperty(string property, string value)
		{
			return WithProperty(property, value, true);
		}

		public bool Contains(string property)
		{
			return Values.ContainsKey(property);
		}

		public IDictionary<string, string> ToDictionary()
		{
			var dictionary = new Dictionary<string, string>(Count, StringComparer.Ordinal);
			foreach (var kv in Values)
			{
				dictionary.TryAdd(kv.Key, kv.Value.ToString().ToLowerInvariant());
			}
			return dictionary;
		}
		
		public bool TryGetValue(string property, out string value)
		{
			return Values.TryGetValue(property, out value);
		}

		public bool Equals(BlockState other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return ID == other.ID;
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			foreach (var kv in Values)
			{
				yield return kv;
			}
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
		
		private static readonly Regex VariantParser = new Regex("(?'property'[^=,]*?)=(?'value'[^,]*)", RegexOptions.Compiled);
		public static Dictionary<string, string> ParseData(string variant)
		{
			var match = VariantParser.Match(variant);

			Dictionary<string, string> values  = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (match.Success)
			{
				Match lastMatch = null;

				do
				{
					//	var match = matches[i];
					values.Add(match.Groups["property"].Value, match.Groups["value"].Value);

					lastMatch = match;
					match = match.NextMatch();

					if (!match.Success)
						break;
				} while (match != lastMatch);
			}

			//values[match.Groups["property"].Value] = match.Groups["value"].Value;

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
		//		ResolveModel = ResolveModel,
				Model = Model,
				Default = Default,
			//	Location = Location == null ? null : new ResourceLocation(Location.Namespace, Location.Path),
		//		AppliedModels = AppliedModels,
				IsMultiPart = IsMultiPart,
				//	MultiPartHelper = MultiPartHelper
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

		public string this[string property]
		{
			get
			{
				return Values[property];
			}
		}
	}
}
