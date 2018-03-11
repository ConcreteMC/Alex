using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Alex.API.Blocks.Properties;
using Alex.API.Blocks.State;
using Alex.Blocks;
using Alex.Graphics.Models;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.BlockStates;
using log4net;
using MiNET.Worlds;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using BlockFace = MiNET.BlockFace;

namespace Alex
{
    public static class BlockFactory
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(BlockFactory));

	    public static IReadOnlyDictionary<uint, IBlockState> AllBlockstates => new ReadOnlyDictionary<uint, IBlockState>(RegisteredBlockStates);
	    private static readonly Dictionary<uint, IBlockState> RegisteredBlockStates = new Dictionary<uint, IBlockState>();

		private static readonly Dictionary<uint, Block> RegisteredBlocks = new Dictionary<uint, Block>();
	    private static readonly Dictionary<uint, BlockModel> ModelCache = new Dictionary<uint, BlockModel>();

		private static readonly Dictionary<int, BlockMeta> CachedBlockMeta = new Dictionary<int, BlockMeta>();

	    private static ResourcePackLib.Json.Models.BlockModel CubeModel { get; set; }
	    private static readonly LiquidBlockModel StationairyWaterModel = new LiquidBlockModel()
	    {
		    IsFlowing = false,
		    IsLava = false,
		    Level = 8
	    };

	    private static readonly LiquidBlockModel FlowingWaterModel = new LiquidBlockModel()
	    {
		    IsFlowing = true,
		    IsLava = false,
		    Level = 8
	    };

	    private static readonly LiquidBlockModel StationairyLavaModel = new LiquidBlockModel()
	    {
		    IsFlowing = false,
		    IsLava = true,
		    Level = 8
	    };

	    private static readonly LiquidBlockModel FlowingLavaModel = new LiquidBlockModel()
	    {
		    IsFlowing = true,
		    IsLava = true,
		    Level = 8
	    };

		internal static void Init()
	    {
		    JArray blockArray = JArray.Parse(Encoding.UTF8.GetString(Resources.blocks));
		    Dictionary<string, JObject> blockMetaDictionary =
			    JsonConvert.DeserializeObject<Dictionary<string, JObject>>(
				    Encoding.UTF8.GetString(Resources.blockstates_without_models_pretty));
		    foreach (var item in blockArray)
		    {
			    byte id = 0;
			    bool transparent = false;
			    string name = string.Empty;
			    string displayName = string.Empty;

			    foreach (dynamic entry in item)
			    {
				    if (entry.Name == "id")
				    {
					    id = entry.Value;
				    }
				    else if (entry.Name == "transparent")
				    {
					    transparent = entry.Value;
				    }
				    else if (entry.Name == "name")
				    {
					    name = entry.Value;
				    }
				    else if (entry.Name == "displayName")
				    {
					    displayName = entry.Value;
				    }
			    }

			    if (id == 0 || string.IsNullOrWhiteSpace(name)) continue;

			    BlockMeta meta = new BlockMeta
			    {
					ID = id,
				    Transparent = transparent,
				    DisplayName = displayName,
					Name = name
			    };

				JObject found = blockMetaDictionary
				    .FirstOrDefault(x => x.Key.StartsWith($"minecraft:{name}", StringComparison.InvariantCultureIgnoreCase)).Value;
			    if (found != null)
			    {
				    meta.AmbientOcclusionLightValue = found["ambientOcclusionLightValue"].Value<double>();
				    meta.IsFullBlock = found["isFullBlock"].Value<bool>();
				    meta.LightOpacity = found["lightOpacity"].Value<int>();
				    meta.LightValue = found["lightValue"].Value<int>();
				    meta.IsBlockNormalCube = found["isBlockNormalCube"].Value<bool>();
				    meta.IsSideSolid = found["isSideSolid"].ToObject<Dictionary<string, bool>>();
				    meta.IsFullCube = found["isFullCube"].Value<bool>();
			    }

			    MiNET.Blocks.Block minetBlock = MiNET.Blocks.BlockFactory.GetBlockByName(name);

			    if (minetBlock == null)
			    {
				    minetBlock = MiNET.Blocks.BlockFactory.GetBlockById(id); 
			    }

			    if (minetBlock != null)
			    {
				    meta.Solid = minetBlock.IsSolid;
				    meta.FrictionFactor = minetBlock.FrictionFactor;
				    meta.Replacible = minetBlock.IsReplacible;

				    if (minetBlock.IsTransparent && !meta.Transparent)
				    {
					    meta.Transparent = true;
				    }
				}

			    CachedBlockMeta.TryAdd(id, meta);
		    }
		}

	    private static BlockModel GetOrCacheModel(ResourceManager resources, McResourcePack resourcePack, IBlockState state)
	    {
		    if (ModelCache.TryGetValue(state.ID, out var r))
		    {
			    return r;
			}
		    else
		    {
				var result = GetModelResolver(resourcePack, state);
			    if (result == null)
			    {
				    return null;
			    }

				var v = result.Invoke(resources);

				ModelCache.TryAdd(state.ID, v);
			    return v;
		    }
		    //return _modelCache.GetOrAdd(state, u => modelCreator.Invoke(resources));
	    }

	    public partial class TableEntry
	    {
		    [JsonProperty("runtimeID")]
		    public uint RuntimeId { get; set; }

		    [JsonProperty("name")]
		    public string Name { get; set; }

		    [JsonProperty("id")]
		    public long Id { get; set; }

		    [JsonProperty("data")]
		    public long Data { get; set; }

		    public static TableEntry[] FromJson(string json) => JsonConvert.DeserializeObject<TableEntry[]>(json);
		}

	    private static bool _builtin = false;
	    private static void RegisterBuiltinBlocks()
	    {
		    if (_builtin)
			    return;

		    _builtin = true;

			//RegisteredBlockStates.Add(Block.GetBlockStateID(), StationairyWaterModel);
	    }

		internal static int LoadResources(ResourceManager resources, McResourcePack resourcePack, bool replace,
		    bool reportMissing = false)
	    {
		    if (resourcePack.TryGetBlockModel("cube_all", out ResourcePackLib.Json.Models.BlockModel cube))
		    {
			    cube.Textures["all"] = "no_texture";
			    CubeModel = cube;
		    }

			RegisterBuiltinBlocks();

		    return LoadModels(resources, resourcePack, replace, reportMissing);
	    }

	    private static int LoadModels(ResourceManager resources, McResourcePack resourcePack, bool replace,
		    bool reportMissing)
	    {
			TableEntry[] tablesEntries = TableEntry.FromJson(Resources.runtimeid_table);
			int importCounter = 0;

		    uint c = 0;
		    foreach (var bs in resourcePack.BlockStates)
		    {
			    byte metaId = 0;

			    var entries = tablesEntries.Where(x =>
			    x.Name.Equals("minecraft:" + bs.Key, StringComparison.InvariantCultureIgnoreCase)).ToArray();

			    if (entries.Length == 0)
			    {
				    if (reportMissing)
					    Log.Warn($"Could not resolve block id for blockstate minecraft:{bs.Key}");

					continue;
			    }

			    foreach (var variant in bs.Value.Variants)
			    {
				    string name = bs.Key;
				    //if (metaId > entries.Length - 1)
				    //{
					//    Log.Warn($"Entry out of range: {name} ({metaId} > {entries.Length - 1})");
					//    metaId++;
					//    continue;
				    //}
					

					IBlockState blockStateData = Blocks.State.BlockState.FromString(variant.Key);
				    blockStateData.Name = name;

				    var tableEntry = entries[metaId % entries.Length];


					var blockId = (int)tableEntry.Id;

				    uint id = c++;
				    byte metadata = metaId;
				    //uint id = tableEntry.RuntimeId; //Block.GetBlockStateID(blockId, metadata);
				    metaId++;

				    if (RegisteredBlockStates.TryGetValue(id, out IBlockState s))
				    {
						Log.Warn($"Duplicate blockstate id (Existing: {s.Name}[{s.ToString()}] | New: {name}[{blockStateData.ToString()}]) ");
						continue;
				    }

				    blockStateData.ID = id;
					
				    var cached = GetOrCacheModel(resources, resourcePack, blockStateData);
				    if (cached == null)
				    {
					    if (reportMissing)
						    Log.Warn($"Missing blockmodel for blockstate {name}[{variant.Key}]");

					    continue;
				    }

					if (Load(blockStateData, () =>
				    {
					    var block = new Block(id);
					  //  if (blockId == 69)
					 //   {
					//	    block = new Lever(id);
					 //   }
					 //   else if (blockId == 64 || (blockId >= 193 && blockId <= 197))
					 //   {
					//	    block = new WoodenDoor(blockId, (byte) tableEntry.Data);
					 //   }

					    BlockMeta knownMeta;
					   // if (!CachedBlockMeta.TryGetValue(c, out knownMeta))
					    {
						    knownMeta = CachedBlockMeta
							    .FirstOrDefault(x => x.Value.Name.Equals(bs.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
						    if (knownMeta == null)
						    {
							    knownMeta = new BlockMeta
							    {
								    Transparent = false,
								    DisplayName = bs.Key,
									Solid = true
							    };
						    }
						    else
						    {
							    //   blockId = knownMeta.ID;
						    }
					    }

					    block.BlockModel = cached;
					    block.Transparent = knownMeta.Transparent;
					    block.DisplayName = knownMeta.DisplayName;
					    block.LightValue = knownMeta.LightValue;
					    block.AmbientOcclusionLightValue = knownMeta.AmbientOcclusionLightValue;
					    block.LightOpacity = knownMeta.LightOpacity;
					    block.IsBlockNormalCube = knownMeta.IsBlockNormalCube;
					    block.IsFullCube = knownMeta.IsFullCube;
					    block.IsFullBlock = knownMeta.IsFullBlock;
					    block.BlockState = blockStateData;
					    block.Solid = knownMeta.Solid;
					    block.Drag = knownMeta.FrictionFactor;
					    block.IsReplacible = knownMeta.Replacible;


					    blockStateData.SetBlock(block);
					    RegisteredBlockStates.TryAdd(id, blockStateData);

					    return block;
				    }, replace))
				    {
					    importCounter++;
				    }
			    }
		    }

			return importCounter;
		}

	    private static bool Load(IBlockState id, Func<Block> blockFunction, bool replace)
	    {
		    if (replace)
		    {
			    if (RegisteredBlocks.ContainsKey(id.ID))
			    {
				    RegisteredBlocks[id.ID] = blockFunction();
			    }
			    else
			    {
					RegisteredBlocks.Add(id.ID, blockFunction());
			    }

			    return true;
		    }

		    return RegisteredBlocks.TryAdd(id.ID, blockFunction());
	    }

		//private static string Get

	    private static string FixBlockStateNaming(string name, IBlockState data)
	    {
			string color = null;
			data.TryGetValue("color", out color);

			string variant = null;
			data.TryGetValue("variant", out variant);

			string type = null;
			data.TryGetValue("type", out type);
			int level = 8;
			if (data.TryGetValue("level", out string lvl))
			{
				if (int.TryParse(lvl, out level))
				{

				}
			}

			//string half = null;
			//data.TryGetValue("half", out half);

			if (name.Contains("wooden_slab") && !string.IsNullOrWhiteSpace(variant))
			{
				if (!string.IsNullOrWhiteSpace(variant))
				{
					name = $"{variant}_slab";
				}
			}
			else if (name.Contains("leaves") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_leaves";
			}
			else if (name.Contains("log") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_log";
			}
			else if (name.StartsWith("red_flower") && !string.IsNullOrWhiteSpace(type))
			{
				name = $"{type}";
			}
			else if (name.StartsWith("yellow_flower") && !string.IsNullOrWhiteSpace(type))
			{
				name = $"{type}";
			}
			else if (name.StartsWith("sapling") && !string.IsNullOrWhiteSpace(type))
			{
				name = $"{type}_sapling";
			}
			else if (name.StartsWith("planks") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_planks";
			}
			else if (name.StartsWith("double_stone_slab") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_double_slab";
			}
			else if (name.StartsWith("double_plant") && !string.IsNullOrWhiteSpace(variant))
			{
				if (variant.Equals("sunflower", StringComparison.InvariantCultureIgnoreCase))
				{
					name = "sunflower";
				}
				else if (variant.Equals("paeonia", StringComparison.InvariantCultureIgnoreCase))
				{
					name = "paeonia";
				}
				else if (variant.Equals("syringa", StringComparison.InvariantCultureIgnoreCase))
				{
					name = "syringa";
				}
				else
				{
					name = $"double_{variant}";
				}
			}
			else if (name.StartsWith("deadbush"))
			{
				name = "dead_bush";
			}
			else if (name.StartsWith("tallgrass"))
			{
				name = "tall_grass";
			}
			else if (!string.IsNullOrWhiteSpace(color))
			{
				name = $"{color}_{name}";
			}

			/*if (name.Equals("water", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = StationairyWaterModel;
					w.Level = level;
					return w;
				};
			}
			else if (name.Equals("flowing_water", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = FlowingWaterModel;
					w.Level = level;
					return w;
				};
			}

			if (name.Equals("lava", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = StationairyLavaModel;
					w.Level = level;
					return w;
				};
			}
			else if (name.Equals("flowing_lava", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = FlowingLavaModel;
					w.Level = level;
					return w;
				};
			}*/

		    return name;
	    }

	    private static Func<ResourceManager, BlockModel> GetModelResolver(McResourcePack resourcePack,
		    IBlockState state)
	    {
		    string name = state.Name;

		    if (string.IsNullOrWhiteSpace(name))
		    {
				Log.Warn($"State name is null!");
			    return null;
		    }

		    BlockState blockState;

			if (resourcePack.BlockStates.TryGetValue(name, out blockState) || resourcePack.BlockStates.TryGetValue(FixBlockStateNaming(name, state), out blockState))
			{
				if (blockState.Variants == null ||
				    blockState.Variants.Count == 0)
					return null;

			    if (blockState.Variants.Count == 1)
			    {
				    var v = blockState.Variants.FirstOrDefault();
				    return r => new CachedResourcePackModel(r, new[] {v.Value.FirstOrDefault()});
			    }

			    BlockStateVariant blockStateVariant = null;

			    var data = state.ToDictionary();
			    int closestMatch = int.MinValue;
			    KeyValuePair<string, BlockStateVariant> closest = default(KeyValuePair<string, BlockStateVariant>);
			    foreach (var v in blockState.Variants)
			    {
				    var variantBlockState = Blocks.State.BlockState.FromString(v.Key);

				    int matches = 0;
				    foreach (var kv in data)
				    {
					    if (variantBlockState.TryGetValue(kv.Key.Name, out string vValue))
					    {
						    if (vValue.Equals(kv.Value, StringComparison.InvariantCultureIgnoreCase))
						    {
							    matches++;
						    }
					    }
				    }

				    if (matches > closestMatch)
				    {
					    closestMatch = matches;
					    closest = v;
				    }
			    }

			    blockStateVariant = closest.Value;

			    if (blockStateVariant == null)
			    {
				    var a = blockState.Variants.FirstOrDefault();
				    blockStateVariant = a.Value;
			    }


			    var subVariant = blockStateVariant.FirstOrDefault();
			    return r => new CachedResourcePackModel(r, new[] {subVariant});
		    }

		    if (blockState != null && blockState.Parts != null && blockState.Parts.Length > 0)
		    {
			    return m => new MultiStateResourcePackModel(m, blockState);
		    }

		    return null;
	    }

	    private static readonly IBlockState AirState = new Blocks.State.BlockState();

	    public static IBlockState GetBlockState(int blockId, byte meta)
	    {
		    return GetBlockState(Block.GetBlockStateID(blockId, meta));
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

	   /* public static IBlockState GetBlockState(IBlockState state)
	    {
		    var data = Blocks.State.BlockState.FromString(state.ToString());
			IBlockState closest = null;

		    int closestMatch = 0;
			foreach (var registeredBlockstate in RegisteredBlockStates.Where(x => x.Value.Name.Equals(state.Name, StringComparison.InvariantCultureIgnoreCase) /*&& !x.Value.GetBlock().Equals(state.GetBlock())))
			{
				var rawVariantData = registeredBlockstate.Value.ToString();
				var variantData = Blocks.State.BlockState.FromString(rawVariantData);

			    int matches = 0;
			    foreach (var kv in data)
			    {
				    if (variantData.TryGetValue(kv.Key, out string v))
				    {
						if (v.Equals(kv.Value, StringComparison.InvariantCultureIgnoreCase))
					    {
						    matches++;
					    }
				    }
			    }

				if (matches == data.Count)
				{
					return registeredBlockstate.Value;
				}

			    if (matches > closestMatch)
			    {
				    closestMatch = matches;
				    closest = registeredBlockstate.Value;
			    }

			}

		    if (closestMatch < (data.Count / 2))
		    {
			    return state;
		    }

		    return closest;
		    // var first = RegisteredBlockStates.FirstOrDefault(x => x.Value.ToDictionary() == state).Key;

		    // return first;
	    }*/

	    public static uint GetBlockStateId(IBlockState state)
	    {
		    var first = RegisteredBlockStates.FirstOrDefault(x => x.Value.Equals(state)).Key;

		    return first;

	    }

		private static Block Air { get; } = new Air();
	    public static Block GetBlock(uint palleteId)
	    {
		    if (palleteId == 0) return Air;
			if (RegisteredBlocks.TryGetValue(palleteId, out Block b))
		    {
			    return b;
		    }

		    return new Block(palleteId)
		    {
			    BlockModel = new ResourcePackModel(null, new[] { new BlockStateModel
			    {
					Model = CubeModel,
					ModelName = CubeModel.Name,
					Y = 0,
					X = 0,
					Uvlock = false,
					Weight = 0
				}}),
			    Transparent = false,
			    DisplayName = "Unknown"
		    };
		}

	    public static Block GetBlock(int id, byte metadata)
		{
			if (id == 0) return Air;
			return GetBlock(Block.GetBlockStateID(id, metadata));
		}

	    private class BlockMeta
	    {
		    public int ID = -1;
		    public string Name;
		    public string DisplayName;
		    public bool Transparent;
		    public bool IsFullBlock;
		    public double AmbientOcclusionLightValue = 1.0;
		    public int LightValue;
		    public int LightOpacity;
		    public bool IsBlockNormalCube;
		    public bool IsFullCube;
		    public bool Solid;
		    public float FrictionFactor;

		    public Dictionary<string, bool> IsSideSolid;
		    public bool Replacible;
	    }
    }
}
