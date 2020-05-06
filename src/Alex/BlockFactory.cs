using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alex.API.Blocks.State;
using Alex.API.Resources;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using BlockModel = Alex.Graphics.Models.Blocks.BlockModel;

namespace Alex
{
	public static class BlockFactory
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockFactory));

		public static IReadOnlyDictionary<uint, IBlockState> AllBlockstates => new ReadOnlyDictionary<uint, IBlockState>(RegisteredBlockStates);
		public static IReadOnlyDictionary<string, BlockStateVariantMapper> AllBlockstatesByName => new ReadOnlyDictionary<string, BlockStateVariantMapper>(BlockStateByName);

		//private static readonly Dictionary<uint, Block> BlockByBlockStateId = new Dictionary<uint, Block>();
		private static readonly Dictionary<uint, IBlockState> RegisteredBlockStates = new Dictionary<uint, IBlockState>();
		private static readonly Dictionary<string, BlockStateVariantMapper> BlockStateByName = new Dictionary<string, BlockStateVariantMapper>();
		private static readonly Dictionary<uint, BlockModel> ModelCache = new Dictionary<uint, BlockModel>();
		private static readonly Dictionary<long, string> ProtocolIdToBlockName = new Dictionary<long, string>();
		private static ResourcePackLib.Json.Models.Blocks.BlockModel CubeModel { get; set; }
		public static readonly LiquidBlockModel StationairyWaterModel = new LiquidBlockModel()
		{
			IsFlowing = false,
			IsLava = false,
			Level = 8
		};

		public static readonly LiquidBlockModel StationairyLavaModel = new LiquidBlockModel()
		{
			IsFlowing = false,
			IsLava = true,
			Level = 8
		};

		private static BlockModel GetOrCacheModel(ResourceManager resources, McResourcePack resourcePack, IBlockState state, uint id, bool rebuild)
		{
			BlockModel result = null;
			if (ModelCache.TryGetValue(id, out result) && !rebuild)
			{
                return result;
			}
			else
			{
				var r = ResolveModel(resources, resourcePack, state);
				if (r == null)
				{
					return result;
				}

				result = r;

                /*if (state.GetTypedValue(WaterLoggedProperty))
				{
					result = new MultiBlockModel(result, StationairyWaterModel);
				}*/

				if (!ModelCache.TryAdd(id, result))
				{
					if (rebuild)
					{
						ModelCache[id] = result;
					}
					else
					{
						Log.Warn($"Could not register model in cache! {state.Name} - {state.ID}");
					}
				}

				return result;
			}
		}

        private static bool _builtin = false;
		private static void RegisterBuiltinBlocks()
		{
			if (_builtin)
				return;

			_builtin = true;
			
			//RegisteredBlockStates.Add(Block.GetBlockStateID(), StationairyWaterModel);
		}

		public static TableEntry[] RuntimeIdTable { get; private set; }

		internal static int LoadResources(IRegistryManager registryManager, ResourceManager resources, McResourcePack resourcePack, bool replace,
			bool reportMissing = false, IProgressReceiver progressReceiver = null)
		{
			var raw = ResourceManager.ReadStringResource("Alex.Resources.runtimeidtable.json");

			RuntimeIdTable = TableEntry.FromJson(raw);

			var blockEntries = resources.Registries.Blocks.Entries;

			progressReceiver?.UpdateProgress(0, "Loading block registry...");
			for (int i = 0; i < blockEntries.Count; i++)
			{
				var kv = blockEntries.ElementAt(i);

				progressReceiver?.UpdateProgress(i * (100 / blockEntries.Count), "Loading block registry...",
					kv.Key);

				ProtocolIdToBlockName.TryAdd(kv.Value.ProtocolId, kv.Key);
			}

			progressReceiver?.UpdateProgress(0, "Loading block models...");

			if (resourcePack.TryGetBlockModel("cube_all", out ResourcePackLib.Json.Models.Blocks.BlockModel cube))
			{
				cube.Textures["all"] = "no_texture";
				CubeModel = cube;

				UnknownBlockModel = new CachedResourcePackModel(resources, new BlockStateModel[]
				{
					new BlockStateModel()
					{
						Model = CubeModel,
						ModelName = "Unknown model",
					}
				});

				AirState.Model = UnknownBlockModel;
			}

			RegisterBuiltinBlocks();

			return LoadModels(registryManager, resources, resourcePack, replace, reportMissing, progressReceiver);
		}

		private static PropertyBool WaterLoggedProperty = new PropertyBool("waterlogged");
		public static BlockModel UnknownBlockModel { get; set; }

		private static int LoadModels(IRegistryManager registryManager, ResourceManager resources,
			McResourcePack resourcePack, bool replace,
			bool reportMissing, IProgressReceiver progressReceiver)
		{
			long idCounter = 0;
			var blockRegistry = registryManager.GetRegistry<Block>();

			var data = BlockData.FromJson(ResourceManager.ReadStringResource("Alex.Resources.NewBlocks.json"));
			int total = data.Count;
			int done = 0;
			int importCounter = 0;
			int multipartBased = 0;

			uint c = 0;
			foreach (var entry in data)
			{
				double percentage = 100D * ((double) done / (double) total);
				progressReceiver.UpdateProgress((int) percentage, $"Importing block models...", entry.Key);

				var variantMap = new BlockStateVariantMapper();
				var defaultState = new BlockState
				{
					Name = entry.Key,
					VariantMapper = variantMap
				};

				if (entry.Value.Properties != null)
				{
					foreach (var property in entry.Value.Properties)
					{
						defaultState = (BlockState) defaultState.WithPropertyNoResolve(property.Key,
							property.Value.FirstOrDefault(), false);
					}

				//	defaultState = (BlockState)defaultState.WithPropertyNoResolve("test", "a", false);
				}

				foreach (var s in entry.Value.States)
				{
					var id = s.ID;

					BlockState variantState = (BlockState) (defaultState).CloneSilent();
					variantState.ID = id;
					variantState.Name = entry.Key;
					//variantState.VariantMapper = variantMap;

					if (s.Properties != null)
					{
						foreach (var property in s.Properties)
						{
							variantState =
								(Blocks.State.BlockState) variantState.WithPropertyNoResolve(property.Key,
									property.Value, false);
						}
					}

					//	resourcePack.BlockStates.TryGetValue(entry.Key)
					if (!replace && RegisteredBlockStates.TryGetValue(id, out IBlockState st))
					{
						Log.Warn(
							$"Duplicate blockstate id (Existing: {st.Name}[{st.ToString()}] | New: {entry.Key}[{variantState.ToString()}]) ");
						continue;
					}


					var cachedBlockModel = GetOrCacheModel(resources, resourcePack, variantState, id, replace);
					if (cachedBlockModel == null)
					{
						//if (reportMissing)
						Log.Warn($"Missing blockmodel for blockstate {entry.Key}[{variantState.ToString()}]");

						cachedBlockModel = UnknownBlockModel;
					}

					if (variantState.IsMultiPart) multipartBased++;

					string displayName = entry.Key;
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
					
					variantState.Model = cachedBlockModel;
					variantState.Default = s.Default;

					if (string.IsNullOrWhiteSpace(block.DisplayName) ||
					    !block.DisplayName.Contains("minet", StringComparison.InvariantCultureIgnoreCase))
					{
						block.DisplayName = displayName;
					}

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
						Log.Warn($"Could not add variant to variant map: {variantState.Name}[{variantState.ToString()}]");
					}
				}
				//	variantMap.

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

				done++;
			}

			Log.Info($"Got {multipartBased} multi-part blockstate variants!");
			return importCounter;
		}
		
		private static BlockModel ResolveModel(ResourceManager resources, McResourcePack resourcePack,
			IBlockState state)
		{
			string name = state.Name;

			if (string.IsNullOrWhiteSpace(name))
			{
				Log.Warn($"State name is null!");
				return null;
			}

			if (name.Contains("water"))
			{
				return StationairyWaterModel;
			}

			if (name.Contains("lava"))
			{
				return StationairyLavaModel;
			}

			BlockStateResource blockStateResource;

			if (resourcePack.BlockStates.TryGetValue(name, out blockStateResource))
			{
				if (blockStateResource != null && blockStateResource.Parts != null && blockStateResource.Parts.Length > 0 && blockStateResource.Parts.All(x => x.Apply.All(b => b.Model != null)))
				{
					var models = MultiPartModels.GetModels(state, blockStateResource);
					
					if (state is BlockState ss)
					{
						ss.MultiPartHelper = blockStateResource;
						ss.IsMultiPart = true;
						ss.AppliedModels = models.Select(x => x.ModelName).ToArray();
					}
					
					return new CachedResourcePackModel(resources, models);
				}

				if (blockStateResource?.Variants == null ||
					blockStateResource.Variants.Count == 0)
					return null;

				if (blockStateResource.Variants.Count == 1)
				{
					var v = blockStateResource.Variants.FirstOrDefault();
					if (v.Value == null)
					{
						return null;
					}

					var models = v.Value.Where(x => x.Model?.Elements != null).ToArray();

					if (models.Length == 0)
					{
						return null;
					}

					return new CachedResourcePackModel(resources, models, v.Value.ToArray().Length > 1);
				}

				BlockStateVariant blockStateVariant = null;

				var data = state.ToDictionary();
				int closestMatch = 0;
				KeyValuePair<string, BlockStateVariant> closest = default(KeyValuePair<string, BlockStateVariant>);
				foreach (var v in blockStateResource.Variants)
				{
					int matches = 0;
					var variantBlockState = Blocks.State.BlockState.FromString(v.Key);

					bool isInvalid = false;

					/*if (state.ExactMatch(variantBlockState))
					{
						closest = v;
						break;
					}*/
					
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
								isInvalid = true;
								break;
							}
						}
						else
						{
							isInvalid = true;
							break;
						}
					}
					
					//if (isInvalid)
					//	continue;

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
				
				if (asArray.Length == 0 || asArray.Any(x => x.Model == null || x.Model.Elements == null))
				{
					return null;
				}
				
				return new CachedResourcePackModel(resources, asArray, asArray.Length > 1);
			}

			return null;
		}

		private static readonly IBlockState AirState = new BlockState(){Name = "Unknown"};

		public static IBlockState GetBlockState(string palleteId)
		{
			if (BlockStateByName.TryGetValue(palleteId, out var result))
			{
				return result.GetDefaultState();
			}

			return AirState;
		}

		public static IBlockState GetBlockState(uint palleteId)
		{
			if (RegisteredBlockStates.TryGetValue(palleteId, out var result))
			{
				return result;
			}

			return AirState;
		}
/*
		public static IBlockState GetBlockState(int palleteId)
		{
			if (RegisteredBlockStates.TryGetValue((uint)palleteId, out var result))
			{
				return result;
			}

			return AirState;
		}
*/
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
