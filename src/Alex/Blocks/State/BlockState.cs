using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Common.Blocks.Properties;
using Alex.Common.Resources;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.Utils;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using NLog;

namespace Alex.Blocks.State
{
	public class BlockState : IRegistryEntry<BlockState>, IEnumerable<StateProperty>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockState));
		
		protected  internal  List<StateProperty> States { get; set; }
		public int Count => States.Count;
		public BlockState()
		{
			States = new List<StateProperty>();
		}

		public string Name { get; set; }
		public uint ID { get; set; }

		public BlockStateVariant ModelData { get; set; } = new BlockStateVariant()
		{
			new BlockStateModel()
		};
		
		public Block Block       { get; set; } = new Air();
		public bool  Default     { get; set; } = false;

		public BlockStateVariantMapper VariantMapper { get; set; }

		internal BoundingBox[] BoundingBoxes { get; set; } = null;

		public T GetTypedValue<T>(StateProperty<T> property)
		{
			var first = States.FirstOrDefault(
				x => x.Identifier == property.Identifier);

			if (first == null)
				return default(T);

			if (first is StateProperty<T> ip)
				return ip.Value;

			return property.ParseValue(first.StringValue);
		}

		public BlockState WithProperty(string property, string value, params string[] requiredMatches)
		{
			if (VariantMapper.TryResolve(this, property, value, out BlockState result, requiredMatches))
			{
				return result;
			}

			if (LoggingConstants.LogInvalidBlockProperties)
				Log.Debug($"Invalid property on state {Name} ({property}={value})");
			
			return this;
		}

		public bool TryGetValue(string property, out string value)
		{
			var first = States.FirstOrDefault(x => x.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));

			if (first != null)
			{
				value = first.StringValue;
				return true;
			}

			value = null;
			return false;
		}

		public bool Equals(BlockState other)
		{
			bool result = Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
			if (!result) return false;

			var thisStates = new HashSet<StateProperty>(States);
			var otherStates = new HashSet<StateProperty>(other.States);

			otherStates.IntersectWith(thisStates);
			result = otherStates.Count == thisStates.Count;

			return result;
		}

		/// <inheritdoc />
		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			
			return obj.GetHashCode().Equals(GetHashCode());
		}

		public override int GetHashCode()
		{
			var hash = new HashCode();
			//hash.Add(ID);
			hash.Add(Name);
			foreach (var state in States)
			{
				switch (state)
				{
					case PropertyByte blockStateByte:
						hash.Add(blockStateByte);
						break;
					case PropertyInt blockStateInt:
						hash.Add(blockStateInt);
						break;
					case PropertyString blockStateString:
						hash.Add(blockStateString);
						break;
					default:
						hash.Add(state);
						break;
				}
			}
			
			return hash.ToHashCode();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public IEnumerator<StateProperty> GetEnumerator()
		{
			foreach (var kv in States)
			{
				yield return kv;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			var v = States.ToArray();
			for (var index = 0; index < v.Length; index++)
			{
				var kv = v[index];

				sb.Append(kv.Name);
				sb.Append('=');
				sb.Append(kv.StringValue.ToLowerInvariant());

				if (index != v.Length - 1)
					sb.Append(',');
			}

			return sb.ToString();
		}
		
		private static readonly Regex VariantParser = new Regex("(?'property'[^=,]*?)=(?'value'[^,]*)", RegexOptions.Compiled);

		public static Dictionary<string, string> ParseData(string variant)
		{
			return VariantParser.Matches(variant).ToDictionary(
				x => x.Groups["property"].Value, x => x.Groups["value"].Value);
		}

		public BlockState CloneSilent()
		{
			BlockState bs = new BlockState
			{
				Name = Name,
				ID = ID,
				States = new List<StateProperty>(States),
				Block = Block,
				VariantMapper = VariantMapper,
				//		ResolveModel = ResolveModel,
				Default = Default,
				ModelData = ModelData,
				//Location = new ResourceLocation(Location.Namespace, Location.Path)
				//	Location = Location == null ? null : new ResourceLocation(Location.Namespace, Location.Path),
				//		AppliedModels = AppliedModels,
				//IsMultiPart = IsMultiPart,
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
	}
}
