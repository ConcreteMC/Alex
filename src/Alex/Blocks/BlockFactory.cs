using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Alex.API.Resources;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.BlockStates;
using Newtonsoft.Json;
using BlockModel = Alex.Graphics.Models.Blocks.BlockModel;

namespace Alex.Blocks
{
	public static class BlockFactory
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockFactory));

		public static IReadOnlyDictionary<uint, BlockState> AllBlockstates => new ReadOnlyDictionary<uint, BlockState>(RegisteredBlockStates);
		public static IReadOnlyDictionary<string, BlockStateVariantMapper> AllBlockstatesByName => new ReadOnlyDictionary<string, BlockStateVariantMapper>(BlockStateByName);
		
		private static readonly Dictionary<uint, BlockState> RegisteredBlockStates = new Dictionary<uint, BlockState>();
		private static readonly Dictionary<string, BlockStateVariantMapper> BlockStateByName = new Dictionary<string, BlockStateVariantMapper>();
		private static readonly Dictionary<uint, BlockModel> ModelCache = new Dictionary<uint, BlockModel>();
		private static readonly Dictionary<long, string> ProtocolIdToBlockName = new Dictionary<long, string>();
		private static ResourcePackLib.Json.Models.Blocks.BlockModel CubeModel { get; set; }
		public static readonly LiquidBlockModel StationairyWaterModel = new LiquidBlockModel()
		{
			//IsFlowing = false,
			IsLava = false,
		//	Level = 8
		};

		public static readonly LiquidBlockModel StationairyLavaModel = new LiquidBlockModel()
		{
			//IsFlowing = false,
			IsLava = true,
			//Level = 8
		};

		private static BlockModel GetOrCacheModel(ResourceManager resources, McResourcePack resourcePack, BlockState state, uint id, bool rebuild)
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

				UnknownBlockModel = new ResourcePackBlockModel(resources, new BlockStateModel[]
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
		
		public static BlockModel UnknownBlockModel { get; set; }

		private static int LoadModels(IRegistryManager registryManager, ResourceManager resources,
			McResourcePack resourcePack, bool replace,
			bool reportMissing, IProgressReceiver progressReceiver)
		{
			long idCounter = 0;
			var blockRegistry = registryManager.GetRegistry<Block>();
			var blockModelRegistry = registryManager.GetRegistry<BlockModel>();

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
					//	if (property.Key.Equals("waterlogged"))
					//		continue;
						
						defaultState = (BlockState) defaultState.WithPropertyNoResolve(property.Key,
							property.Value.FirstOrDefault(), false);
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
							//if (property.Key.Equals("waterlogged"))
						//		continue;
							
							variantState =
								(Blocks.State.BlockState) variantState.WithPropertyNoResolve(property.Key,
									property.Value, false);
						}
					}

					if (!replace && RegisteredBlockStates.TryGetValue(id, out BlockState st))
					{
						Log.Warn(
							$"Duplicate blockstate id (Existing: {st.Name}[{st.ToString()}] | New: {entry.Key}[{variantState.ToString()}]) ");
						continue;
					}


					var cachedBlockModel = GetOrCacheModel(resources, resourcePack, variantState, id, replace);
					if (cachedBlockModel == null)
					{
						if (reportMissing)
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

				done++;
			}

			Log.Info($"Got {multipartBased} multi-part blockstate variants!");
			return importCounter;
		}

		private static BlockModel ResolveModel(ResourceManager resources, McResourcePack resourcePack,
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
				//	IsFlowing = false,
					IsLava = false,
				//	Level = state.GetTypedValue(Water.LEVEL)
				};
			}

			if (name.Contains("lava"))
			{
				return new LiquidBlockModel()
				{
				//	IsFlowing = false,
					IsLava = true,
				//	Level = state.GetTypedValue(Water.LEVEL)
				};;
			}

			BlockStateResource blockStateResource;

			if (resourcePack.BlockStates.TryGetValue(name, out blockStateResource))
			{
				if (blockStateResource != null && blockStateResource.Parts != null &&
				    blockStateResource.Parts.Length > 0 &&
				    blockStateResource.Parts.All(x => x.Apply.All(b => b.Model != null)))
				{
					var models = MultiPartModels.GetModels(state, blockStateResource);


					state.MultiPartHelper = blockStateResource;
					state.IsMultiPart = true;
					state.AppliedModels = models.Select(x => x.ModelName).ToArray();

					return new ResourcePackBlockModel(resources, models);
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

					var models = v.Value.Where(x => x.Model?.Elements != null && x.Model.Elements.Length > 0).ToArray();

					if (models.Length == 0)
					{
						return null;
					}

					return new ResourcePackBlockModel(resources, models, v.Value.ToArray().Length > 1);
				}

				BlockStateVariant blockStateVariant = null;

				var data = state.ToDictionary();
			//	data.Remove("waterlogged");
				
				int closestMatch = 0;
				KeyValuePair<string, BlockStateVariant> closest = default(KeyValuePair<string, BlockStateVariant>);
				foreach (var v in blockStateResource.Variants)
				{
					int matches = 0;
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

				if (asArray.Length == 0 || asArray.Any(x => x.Model?.Elements == null || x.Model.Elements.Length == 0))
				{
					return null;
				}
				
				return new ResourcePackBlockModel(resources, asArray, asArray.Length > 1);
			}

			return null;
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
