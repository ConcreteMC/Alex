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
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Profiling;

namespace Alex
{
	public static class BlockFactory
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockFactory));

		public static IReadOnlyDictionary<uint, IBlockState> AllBlockstates => new ReadOnlyDictionary<uint, IBlockState>(RegisteredBlockStates);
		public static IReadOnlyDictionary<string, BlockStateVariantMapper> AllBlockstatesByName => new ReadOnlyDictionary<string, BlockStateVariantMapper>(BlockStateByName);

		private static readonly Dictionary<uint, Block> BlockByBlockStateId = new Dictionary<uint, Block>();
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
			if (ModelCache.TryGetValue(id, out var r))
			{
                return r;
			}
			else
			{
				var result = ResolveModel(resources, resourcePack, state);
				if (result == null)
				{
					return null;
				}

                if (state.GetTypedValue(WaterLoggedProperty))
				{
					result = new MultiBlockModel(result, StationairyWaterModel);
				}

				if (!ModelCache.TryAdd(id, result))
				{
					Log.Warn($"Could not register model in cache! {state.Name} - {state.ID}");
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
		private static BlockModel UnknownBlockModel { get; set; }

		private static int LoadModels(IRegistryManager registryManager, ResourceManager resources,
			McResourcePack resourcePack, bool replace,
			bool reportMissing, IProgressReceiver progressReceiver)
		{
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

				var defaultState = new BlockState
				{
					Name = entry.Key
				};

				if (entry.Value.Properties != null)
				{
					foreach (var property in entry.Value.Properties)
					{
						defaultState = (BlockState) defaultState.WithPropertyNoResolve(property.Key,
							property.Value.FirstOrDefault(), false);
					}
				}

				List<BlockState> variants = new List<BlockState>();
				foreach (var s in entry.Value.States)
				{
					var id = s.ID;

					BlockState variantState = (BlockState) (defaultState).CloneSilent();
					variantState.ID = id;
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

					{
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


						/*if (block.IsSourceBlock && !(cachedBlockModel is MultiBlockModel) &&
						    !(cachedBlockModel is LiquidBlockModel))
						{
							if (block.IsWater)
							{
								cachedBlockModel = new MultiBlockModel(cachedBlockModel, StationairyWaterModel);
							}
							else
							{
								cachedBlockModel = new MultiBlockModel(cachedBlockModel, StationairyLavaModel);
							}

							block.Transparent = true;
						}*/

						/*if (variantState.GetTypedValue(WaterLoggedProperty))
						{
							block.Transparent = true;
						}*/

						variantState.Name = entry.Key;
						variantState.Model = cachedBlockModel;
						block.BlockState = variantState;

						if (string.IsNullOrWhiteSpace(block.DisplayName) ||
						    !block.DisplayName.Contains("minet", StringComparison.InvariantCultureIgnoreCase))
						{
							block.DisplayName = displayName;
						}

						variantState.Block = block;
						if (s.Default) //This is the default variant.
						{
							defaultState = variantState;
						}
					}

					variants.Add(variantState);
				}

				var variantMap = new BlockStateVariantMapper();
				variantMap._default = defaultState;

				foreach (var var in variants)
				{
					var.VariantMapper = variantMap;
					if (variantMap.TryAdd(var))
					{
						if (!BlockByBlockStateId.TryAdd(var.ID, (Block) var.Block))
						{
							Log.Warn(
								$"Could not register block, duplicate blockstate id. ID={var.ID} Block name={var.Block.Name}");
						}

						if (!RegisteredBlockStates.TryAdd(var.ID, var))
						{
							if (replace)
							{
								RegisteredBlockStates[var.ID] = var;
								importCounter++;
							}
							else
							{
								Log.Warn(
									$"Failed to add blockstate (variant), key already exists! ({var.ID} - {var.Name})");
							}
						}
						else
						{
							importCounter++;
						}
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
				if (blockStateResource != null && blockStateResource.Parts != null && blockStateResource.Parts.Length > 0)
				{
					if (state is BlockState ss)
					{
						ss.MultiPartHelper = blockStateResource;
						ss.IsMultiPart = true;
					}
					return new CachedResourcePackModel(resources, MultiPartModels.GetModels(state, blockStateResource));
				}

				if (blockStateResource?.Variants == null ||
					blockStateResource.Variants.Count == 0)
					return null;

				if (blockStateResource.Variants.Count == 1)
				{
					var v = blockStateResource.Variants.FirstOrDefault();
					return new CachedResourcePackModel(resources, new[] { v.Value.FirstOrDefault() });
				}

				BlockStateVariant blockStateVariant = null;

				var data = state.ToDictionary();
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
						}
					}

					if (matches > closestMatch || matches == data.Count)
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


				var subVariant = blockStateVariant.FirstOrDefault();
				return new CachedResourcePackModel(resources, new[] { subVariant });
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

		public static IBlockState GetBlockState(int palleteId)
		{
			if (RegisteredBlockStates.TryGetValue((uint)palleteId, out var result))
			{
				return result;
			}

			return AirState;
		}

		public static uint GetBlockStateId(IBlockState state)
		{
			var first = RegisteredBlockStates.FirstOrDefault(x => x.Value.Equals(state)).Key;

			return first;

		}

		public static IBlockState GetBlockStateByProtocolId(long protocolId)
		{
			if (ProtocolIdToBlockName.TryGetValue(protocolId, out string n))
			{
				return GetBlockState(n);
			}

			return AirState;
		}
		
		public static IBlockState GetBlockStateByRuntimeId(long runtimeId)
		{
			var resultR = RuntimeIdTable.FirstOrDefault(x => x.RuntimeId == runtimeId);
			if (resultR == null) return AirState;
			string result = resultR.Name;
			if (result == "minecraft:grass")
			{
				result = "minecraft:grass_block";
			}
			
			return GetBlockState(result);
		}

		public static bool IsBlock(string name)
		{
			return BlockStateByName.ContainsKey(name);
		}

		private static Block Air { get; } = new Air();
		public static Block GetBlock(uint palleteId)
		{
			if (palleteId == 0) return Air;
			if (BlockByBlockStateId.TryGetValue(palleteId, out Block b))
			{
				return b;
			}

			var state = new BlockState()
			{
				Model = new CachedResourcePackModel(null, new[]
				{
					new BlockStateModel
					{
						Model = CubeModel,
						ModelName = CubeModel.Name,
						Y = 0,
						X = 0,
						Uvlock = false,
						Weight = 0
					}
				}),
			};

			var result = new Block(palleteId)
			{
				BlockState = state,
				Transparent = false,
				DisplayName = "Unknown"
			};
			return result;
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
