using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Resources;
using Alex.Blocks.Mapping;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.Utils;
using MiNET.Blocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Block = Alex.Blocks.Minecraft.Block;
using BlockModel = Alex.Graphics.Models.Blocks.BlockModel;
using LightBlock = Alex.Blocks.Minecraft.LightBlock;

namespace Alex.Blocks
{
	public static class BlockFactory
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockFactory));

		public static IReadOnlyDictionary<uint, BlockState> AllBlockstates => new ReadOnlyDictionary<uint, BlockState>(RegisteredBlockStates);
		public static IReadOnlyDictionary<string, BlockStateVariantMapper> AllBlockstatesByName => new ReadOnlyDictionary<string, BlockStateVariantMapper>(BlockStateByName);
		
		private static readonly ConcurrentDictionary<uint, BlockState> RegisteredBlockStates = new ConcurrentDictionary<uint, BlockState>();
		private static readonly ConcurrentDictionary<string, BlockStateVariantMapper> BlockStateByName = new ConcurrentDictionary<string, BlockStateVariantMapper>();

		public static readonly ConcurrentDictionary<string, BlockStateVariantMapper> BedrockStates =
			new ConcurrentDictionary<string, BlockStateVariantMapper>();
		
		//private static readonly ConcurrentDictionary<uint, BlockModel> ModelCache = new ConcurrentDictionary<uint, BlockModel>();
		private static readonly ConcurrentDictionary<long, string> ProtocolIdToBlockName = new ConcurrentDictionary<long, string>();

		private static bool _builtin = false;
		private static void RegisterBuiltinBlocks()
		{
			if (_builtin)
				return;

			_builtin = true;

			var lightBlockVariantMapper = new BlockStateVariantMapper();

			for (byte i = 0; i < 15; i++)
			{
				BlockState bs = new BlockState()
				{
					Default = i == 0,
					Name = "minecraft:light_block",
					VariantMapper = lightBlockVariantMapper,
					Values = new Dictionary<string, string>() {{"block_light_level", i.ToString()}}
				};
				
				var block = new LightBlock()
				{
					LightValue = i
				};
				
				bs.Block = block;
				block.BlockState = bs;
				
				lightBlockVariantMapper.TryAdd(bs);
			}

			BlockStateByName.TryAdd("minecraft:light_block", lightBlockVariantMapper);
			//RegisteredBlockStates.Add(Block.GetBlockStateID(), StationairyWaterModel);
		}
		

		internal static int LoadResources(IRegistryManager registryManager, ResourceManager resources, McResourcePack resourcePack, bool replace,
			bool reportMissing = false, IProgressReceiver progressReceiver = null)
		{
			//RuntimeIdTable = TableEntry.FromJson(raw);

			var blockEntries = resources.Registries.Blocks.Entries;

			progressReceiver?.UpdateProgress(0, "Loading block registry...");
			for (int i = 0; i < blockEntries.Count; i++)
			{
				var kv = blockEntries.ElementAt(i);

				progressReceiver?.UpdateProgress(i, blockEntries.Count, "Loading block registry...",
					kv.Key);

				ProtocolIdToBlockName.TryAdd(kv.Value.ProtocolId, kv.Key);
			}

			progressReceiver?.UpdateProgress(0, "Loading block models...");

			RegisterBuiltinBlocks();

			return LoadModels(registryManager, resources, resourcePack, replace, reportMissing, progressReceiver);
		}
		
		//public static BlockModel UnknownBlockModel { get; set; }

		private static int LoadModels(IRegistryManager registryManager,
			ResourceManager resources,
			McResourcePack resourcePack,
			bool replace,
			bool reportMissing,
			IProgressReceiver progressReceiver)
		{
			var raw = ResourceManager.ReadStringResource("Alex.Resources.blockmap.json");

			var mapping = JsonConvert.DeserializeObject<BlockMap>(raw);
			
			var blockRegistry = registryManager.GetRegistry<Block>();

			var data = BlockData.FromJson(ResourceManager.ReadStringResource("Alex.Resources.NewBlocks.json"));
			int total = data.Count;
			int done = 0;
			int importCounter = 0;
			int multipartBased = 0;

			Parallel.ForEach(
				data, entry =>
				{
					//double percentage = 100D * ((double) done / (double) total);
					progressReceiver.UpdateProgress(done, total, $"Importing block models...", entry.Key);

					if (resourcePack.TryGetBlockState(entry.Key, out var blockStateResource))
					{
						var variantMap   = new BlockStateVariantMapper();
						var defaultState = new BlockState {Name = entry.Key, VariantMapper = variantMap};

						if (entry.Value.Properties != null)
						{
							foreach (var property in entry.Value.Properties)
							{
								defaultState = (BlockState) defaultState.WithPropertyNoResolve(
									property.Key, property.Value.FirstOrDefault(), false);
							}
						}

						foreach (var s in entry.Value.States)
						{
							var id = s.ID;

							BlockState variantState = (BlockState) (defaultState).CloneSilent();
							variantState.ID = id;
							variantState.Name = entry.Key;

							if (s.Properties != null)
							{
								foreach (var property in s.Properties)
								{
									//if (property.Key.ToLower() == "waterlogged")continue;
									variantState = (Blocks.State.BlockState) variantState.WithPropertyNoResolve(
										property.Key, property.Value, false);
								}
							}

							if (!replace && RegisteredBlockStates.TryGetValue(id, out BlockState st))
							{
								Log.Warn(
									$"Duplicate blockstate id (Existing: {st.Name}[{st.ToString()}] | New: {entry.Key}[{variantState.ToString()}]) ");

								continue;
							}

							string                displayName = entry.Key;
							IRegistryEntry<Block> registryEntry;

							if (!blockRegistry.TryGet(entry.Key, out registryEntry))
							{
								registryEntry = new UnknownBlock(id);
								displayName = $"(MISSING) {displayName}";

								registryEntry = registryEntry.WithLocation(entry.Key); // = entry.Key;
							}
							else
							{
								registryEntry = registryEntry.WithLocation(entry.Key);
							}

							var block = registryEntry.Value;

							variantState.Model = ResolveModel(resources, blockStateResource, variantState);
							if (variantState.Model == null)
							{
								Log.Warn($"No model found for {entry.Key}[{variantState.ToString()}]");
							}

							if (variantState.IsMultiPart) multipartBased++;

							variantState.Default = s.Default;

							if (string.IsNullOrWhiteSpace(block.DisplayName))
								block.DisplayName = displayName;

							variantState.Block = block;
							block.BlockState = variantState;

							if (variantMap.TryAdd(variantState))
							{
								if (!RegisteredBlockStates.TryAdd(variantState.ID, variantState))
								{
									if (replace)
									{
										RegisteredBlockStates[variantState.ID] = variantState;
										importCounter++;
									}
									else
									{
										Log.Warn(
											$"Failed to add blockstate (variant), key already exists! ({variantState.ID} - {variantState.Name})");
									}
								}
								else
								{
									importCounter++;
								}
							}
							else
							{
								Log.Warn(
									$"Could not add variant to variant map: {variantState.Name}[{variantState.ToString()}]");
							}
						}

						if (!BlockStateByName.TryAdd(defaultState.Name, variantMap))
						{
							if (replace)
							{
								BlockStateByName[defaultState.Name] = variantMap;
							}
							else
							{
								Log.Warn($"Failed to add blockstate, key already exists! ({defaultState.Name})");
							}
						}
					}
					else
					{
						Log.Warn($"Could not find blockstate with key: {entry.Key}");
					}

					done++;
				});

			Regex regex    = new Regex(@"(?'key'[\:a-zA-Z_\d][^\[]*)(\[(?'data'.*)\])?", RegexOptions.Compiled);
			var   mappings = mapping.GroupBy(x => x.Value.BedrockIdentifier).ToArray();

			int counter = 1;

			Parallel.ForEach(
				mappings, (m) =>
				{
					progressReceiver?.UpdateProgress(counter, mapping.Count, "Mapping blockstates...", m.Key);

					var  mapper = new BlockStateVariantMapper();
					bool first  = true;

					foreach (var state in m)
					{
						var match = regex.Match(state.Key);

						if (!match.Groups["key"].Success)
						{
							Log.Warn($"Entry without key!");

							continue;
						}

						BlockState pcVariant = GetBlockState(match.Groups["key"].Value);

						if (pcVariant != null)
						{
							Dictionary<string, string> properties = null;

							if (match.Groups["data"].Success)
							{
								properties = BlockState.ParseData(match.Groups["data"].Value);

								var p = properties.ToArray();

								for (var i = 0; i < p.Length; i++)
								{
									var prop = p[i];

									if (i == p.Length - 1)
									{
										pcVariant = pcVariant.WithProperty(prop.Key, prop.Value);
									}
									else
									{
										pcVariant = pcVariant.WithPropertyNoResolve(prop.Key, prop.Value);
									}
								}
							}
						}

						if (pcVariant == null)
						{
							Log.Warn($"Map failed: {match.Groups["key"].Value} -> {state.Value.BedrockIdentifier}");

							continue;
						}

						pcVariant = pcVariant.CloneSilent();

						PeBlockState bedrockState = new PeBlockState(pcVariant);
						bedrockState.Name = state.Value.BedrockIdentifier;
						bedrockState.VariantMapper = mapper;
						//bedrockState.AppliedModels = pcVariant.AppliedModels;
						bedrockState.IsMultiPart = pcVariant.IsMultiPart;
					//	bedrockState.MultiPartHelper = pcVariant.MultiPartHelper;
						bedrockState.ResolveModel = pcVariant.ResolveModel;
						bedrockState.Model = pcVariant.Model;
						bedrockState.Block = pcVariant.Block;
						bedrockState.ID = (uint) Interlocked.Increment(ref counter);
						bedrockState.Default = first;

						first = false;

						if (state.Value.BedrockStates != null && state.Value.BedrockStates.Count > 0)
						{
							foreach (var bs in state.Value.BedrockStates)
							{
								bedrockState.Values[bs.Key] = bs.Value.ToString();
							}
						}

						if (!mapper.TryAdd(bedrockState.WithLocation(bedrockState.Name).Value))
						{
							Log.Warn($"Failed to add bedrockstate: {state.Value.BedrockIdentifier}");
						}
					}

					BedrockStates[m.Key] = mapper;
				});

			Log.Info($"Loaded {multipartBased} multi-part blockstate variants!");
			Log.Info($"Loaded {BedrockStates.Count} mappings...");

			return importCounter;
		}

		private static BlockModel ResolveModel(ResourceManager resources,
			BlockStateResource blockStateResource,
			BlockState state)
		{
			string name = state.Name;

			if (string.IsNullOrWhiteSpace(name))
			{
				Log.Warn($"State name is null!");

				return null;
			}

			if (name.Contains("water"))
			{
				return new LiquidBlockModel()
				{
					IsLava = false
				};
			}

			if (name.Contains("lava"))
			{
				return new LiquidBlockModel()
				{
					IsLava = true
				};
			}

			if (blockStateResource != null && blockStateResource.Parts != null && blockStateResource.Parts.Length > 0)
			{
				var models = MultiPartModelHelper.GetModels(state, blockStateResource);

			//	state.MultiPartHelper = blockStateResource;
				state.IsMultiPart = true;
				//state.AppliedModels = models.Select(x => x.ModelName).ToArray();

				return new ResourcePackBlockModel(resources, models);
			}

			if (blockStateResource?.Variants == null || blockStateResource.Variants.Count == 0)
			{
				return null;
			}

			if (blockStateResource.Variants.Count == 1)
			{
				var v = blockStateResource.Variants.FirstOrDefault();

				if (v.Value == null)
				{
					return null;
				}

				var models =
					v.Value.ToArray(); //.Where(x => x.Model?.Elements != null && x.Model.Elements.Length > 0).ToArray();

				if (models.Length == 0)
				{
					return null;
				}

				return new ResourcePackBlockModel(resources, models.ToArray(), v.Value.ToArray().Length > 1);
			}

			BlockStateVariant blockStateVariant = null;

			var data = state.ToDictionary();

			int                                     closestMatch = 0;
			KeyValuePair<string, BlockStateVariant> closest      = default(KeyValuePair<string, BlockStateVariant>);

			foreach (var v in blockStateResource.Variants)
			{
				int matches           = 0;
				var variantBlockState = Blocks.State.BlockState.FromString(v.Key);

				foreach (var kv in data)
				{
					if (variantBlockState.TryGetValue(kv.Key, out string vValue))
					{
						if (vValue.Equals(kv.Value, StringComparison.InvariantCultureIgnoreCase))
						{
							matches++;
						}
						else
						{
							break;
						}
					}
					else
					{
						break;
					}
				}

				if (matches > closestMatch)
				{
					closestMatch = matches;
					closest = v;

					if (matches == data.Count)
						break;
				}
			}

			blockStateVariant = closest.Value;

			if (blockStateVariant == null)
			{
				var a = blockStateResource.Variants.FirstOrDefault();
				blockStateVariant = a.Value;
			}

			var asArray = blockStateVariant.ToArray();

			if (asArray.Length == 0)
			{
				Log.Info($"No elements");

				return null;
			}

			return new ResourcePackBlockModel(resources, asArray, asArray.Length > 1);
		}

		private static readonly BlockState AirState = new BlockState(){Name = "Unknown"};

		public static BlockState GetBlockState(string palleteId)
		{
			if (BlockStateByName.TryGetValue(palleteId, out var result))
			{
				return result.GetDefaultState();
			}

			return AirState;
		}

		public static BlockState GetBlockState(uint palleteId)
		{
			if (RegisteredBlockStates.TryGetValue(palleteId, out var result))
			{
				return result;
			}

			return AirState;
		}

		public static uint GetBlockStateID(int id, byte meta)
		{
			if (id < 0) throw new ArgumentOutOfRangeException();

			return (uint)(id << 4 | meta);
		}

		public static void StateIDToRaw(uint stateId, out int id, out byte meta)
		{
			id = (int)(stateId >> 4);
			meta = (byte)(stateId & 0x0F);
		}

		public partial class TableEntry
		{
			[JsonProperty("runtimeID")]
			public long RuntimeId { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("id")]
			public long Id { get; set; }

			[JsonProperty("data")]
			public long Data { get; set; }

			public static TableEntry[] FromJson(string json)
			{
				return JsonConvert.DeserializeObject<TableEntry[]>(json, new JsonSerializerSettings()
				{
					
				});
			}
		}
    }
}
