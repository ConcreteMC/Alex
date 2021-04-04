using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

		public static IReadOnlyDictionary<uint, BlockState> AllBlockstates => RegisteredBlockStates;
		public static IReadOnlyDictionary<ResourceLocation, BlockStateVariantMapper> AllBlockstatesByName => BlockStateByName;
		
		private static readonly ConcurrentDictionary<uint, BlockState> RegisteredBlockStates = new ConcurrentDictionary<uint, BlockState>();

		private static readonly ConcurrentDictionary<ResourceLocation, BlockStateVariantMapper> BlockStateByName =
			new ConcurrentDictionary<ResourceLocation, BlockStateVariantMapper>();

		public static readonly ConcurrentDictionary<ResourceLocation, BlockStateVariantMapper> BedrockStates = new ConcurrentDictionary<ResourceLocation, BlockStateVariantMapper>();

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
		

		internal static int LoadBlockstates(IRegistryManager registryManager, ResourceManager resources, bool replace,
			bool reportMissing = false, IProgressReceiver progressReceiver = null)
		{
			//RuntimeIdTable = TableEntry.FromJson(raw);

			progressReceiver?.UpdateProgress(0, "Loading block models...");

			RegisterBuiltinBlocks();

			return LoadModels(registryManager, resources, replace, reportMissing, progressReceiver);
		}
		
		//public static BlockModel UnknownBlockModel { get; set; }

		private static readonly Regex _blockMappingRegex = new Regex(@"(?'key'[\:a-zA-Z_\d][^\[]*)(\[(?'data'.*)\])?", RegexOptions.Compiled);
		private static int LoadModels(IRegistryManager registryManager,
			ResourceManager resources,
			bool replace,
			bool reportMissing,
			IProgressReceiver progressReceiver)
		{
			Stopwatch sw  = Stopwatch.StartNew();
			var       raw = ResourceManager.ReadStringResource("Alex.Resources.blockmap.json");

			var mapping = JsonConvert.DeserializeObject<BlockMap>(raw);
			
			var blockRegistry      = registryManager.GetRegistry<Block>();
			//var blockStateRegistry = registryManager.GetRegistry<BlockState>();

			var data = BlockData.FromJson(ResourceManager.ReadStringResource("Alex.Resources.NewBlocks.json"));
			int total = data.Count;
			int done = 0;
			int importCounter = 0;

			void LoadEntry(KeyValuePair<string, BlockData> entry)
			{
				done++;
				if (!resources.TryGetBlockState(entry.Key, out var blockStateResource))
				{
					if (reportMissing)
						Log.Warn($"Could not find blockstate with key: {entry.Key}");

					return;
				}
				
				//double percentage = 100D * ((double) done / (double) total);blockstate variants
				progressReceiver.UpdateProgress(done, total, $"Importing block models...", entry.Key);

				var location     = new ResourceLocation(entry.Key);
				var variantMap   = new BlockStateVariantMapper();
				var defaultState = new BlockState {Name = entry.Key, VariantMapper = variantMap};
				defaultState = defaultState.WithLocation(location).Value;

				if (entry.Value.Properties != null)
				{
					foreach (var property in entry.Value.Properties)
					{
						defaultState = (BlockState) defaultState.WithPropertyNoResolve(property.Key, property.Value.FirstOrDefault(), false);
					}
				}
				
				defaultState.ModelData = ResolveVariant(blockStateResource, defaultState);
				
				variantMap.Model = ResolveModel(resources, blockStateResource, out bool isMultipartModel);
				variantMap.IsMultiPart = isMultipartModel;
				
				if (variantMap.Model == null)
				{
					Log.Warn($"No model found for {entry.Key}[{variantMap.ToString()}]");
				}

				foreach (var s in entry.Value.States)
				{
					if (!replace && RegisteredBlockStates.TryGetValue(s.ID, out BlockState st))
					{
						Log.Warn($"Duplicate blockstate id (Existing: {st.Name}[{st.ToString()}]) ");

						continue;
					}

					BlockState variantState = (BlockState) (defaultState).CloneSilent();
					variantState.ID = s.ID;
					variantState.Name = entry.Key;
					
					if (s.Properties != null)
					{
						foreach (var property in s.Properties)
						{
							//if (property.Key.ToLower() == "waterlogged")continue;
							variantState = (Blocks.State.BlockState) variantState.WithPropertyNoResolve(property.Key, property.Value, false);
						}
					}

					IRegistryEntry<Block> registryEntry;

					if (!blockRegistry.TryGet(location, out registryEntry))
					{
						registryEntry = new UnknownBlock();
						registryEntry = registryEntry.WithLocation(location); // = entry.Key;
					}
					else
					{
						registryEntry = registryEntry.WithLocation(location);
					}

					var block = registryEntry.Value;

					if (string.IsNullOrWhiteSpace(block.DisplayName)) block.DisplayName = entry.Key;

					variantState.ModelData = ResolveVariant(blockStateResource, variantState);
					
					variantState.Block = block.Value;
					block.BlockState = variantState;

				//	if (variantState.IsMultiPart) multipartBased++;

					variantState.Default = s.Default;

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
								Log.Warn($"Failed to add blockstate (variant), key already exists! ({variantState.ID} - {variantState.Name})");
							}
						}
						else
						{
							importCounter++;
						}
					}
					else
					{
						Log.Warn($"Could not add variant to variant map: {variantState.Name}[{variantState.ToString()}]");
					}
				}

				if (!BlockStateByName.TryAdd(location, variantMap))
				{
					if (replace)
					{
						BlockStateByName[location] = variantMap;
					}
					else
					{
						Log.Warn($"Failed to add blockstate, key already exists! ({defaultState.Name})");
					}
				}
			}
			
			Parallel.ForEach(data, LoadEntry);

			var blockStateTime = sw.Elapsed;
			
			var   mappings = mapping.GroupBy(x => x.Value.BedrockIdentifier).ToArray();

			int counter = 1;

			sw.Restart();
			Parallel.ForEach(
				mappings, (m) =>
				{
					progressReceiver?.UpdateProgress(counter, mapping.Count, "Mapping blockstates...", m.Key);

					var  mapper = new BlockStateVariantMapper();
					bool first  = true;

					foreach (var state in m)
					{
						var match     = _blockMappingRegex.Match(state.Key);
						var keyMatch  = match.Groups["key"];
						var dataMatch = match.Groups["data"];
						
						if (!keyMatch.Success)
						{
							Log.Warn($"Entry without key!");

							continue;
						}

						BlockState pcVariant = GetBlockState(keyMatch.Value);

						if (pcVariant != null)
						{
							pcVariant = pcVariant.Clone();
							if (dataMatch.Success)
							{
								var properties = BlockState.ParseData(dataMatch.Value);

								if (properties != null)
								{
									var p = properties.Where(
											x => x.Key != "waterlogged"
											     || (x.Key == "waterlogged" && x.Value == "false"))
									   .ToArray();

									for (var i = 0; i < p.Length; i++)
									{
										var prop = p[i];

										if (i == p.Length - 1)
										{
											pcVariant = pcVariant.WithProperty(prop.Key, prop.Value);
										}
										else
										{
											pcVariant = pcVariant.WithProperty(prop.Key, prop.Value);
										}
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
					//	bedrockState.IsMultiPart = pcVariant.IsMultiPart;
					//	bedrockState.MultiPartHelper = pcVariant.MultiPartHelper;
					//	bedrockState.ResolveModel = pcVariant.ResolveModel;
						//bedrockState.Model = pcVariant.Model;
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

			//Log.Info($"Loaded {multipartBased} multi-part blockstates!");
			Log.Debug($"Loaded {BedrockStates.Count} MC:Java -> MC:Bedrock mappings in {sw.ElapsedMilliseconds}ms...");

			return importCounter;
		}

		private static BlockModel ResolveModel(ResourceManager resources,
			BlockStateResource blockStateResource, out bool isMultipartModel)
		{
			isMultipartModel = blockStateResource.Parts.Any(x => x.When != null && x.When.Length > 0);
			string name = blockStateResource.Name;

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

			return new ResourcePackBlockModel(resources, blockStateResource);
		}

		private static BlockStateVariant ResolveVariant(BlockStateResource blockStateResource, BlockState state)
		{
			if (state.VariantMapper.IsMultiPart)
			{
				return new BlockStateVariant(MultiPartModelHelper.GetModels(state, blockStateResource));
			}
			
			int                                     closestMatch = -1;
			KeyValuePair<string, BlockStateVariant> closest      = default(KeyValuePair<string, BlockStateVariant>);

			foreach (var v in blockStateResource.Variants)
			{
				int matches = 0;
				//var variantBlockState = Blocks.State.BlockState.FromString(v.Key);
				var variant = Blocks.State.BlockState.ParseData(v.Key);

				if (variant != null)
				{
					foreach (var kv in state)
					{
						if (variant.TryGetValue(kv.Key, out string vValue))
						{
							if (vValue.Equals(kv.Value, StringComparison.OrdinalIgnoreCase))
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
				}

				if (matches > closestMatch)
				{
					closestMatch = matches;
					closest = v;

					if (matches == state.Count)
						break;
				}

			}

			return closest.Value;
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
	}
}
