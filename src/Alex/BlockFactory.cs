using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Blocks.State;
using Alex.Blocks;
using Alex.Graphics.Models;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.BlockStates;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex
{
    public static class BlockFactory
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(BlockFactory));

	    private static readonly Dictionary<int, IBlockState> RegisteredBlockStates = new Dictionary<int, IBlockState>();
		private static readonly Dictionary<uint, Block> RegisteredBlocks = new Dictionary<uint, Block>();
		private static readonly Dictionary<int, BlockMeta> CachedBlockMeta = new Dictionary<int, BlockMeta>();
		private static readonly Dictionary<uint, BlockModel> ModelCache = new Dictionary<uint, BlockModel>();

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
				    Transparent = transparent,
				    DisplayName = displayName
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

			    var minetBlock = MiNET.Blocks.BlockFactory.GetBlockById(id);
			    meta.Solid = minetBlock.IsSolid;
			    meta.FrictionFactor = minetBlock.FrictionFactor;
			    meta.Replacible = minetBlock.IsReplacible;

				if (minetBlock.IsTransparent && !meta.Transparent)
			    {
				    meta.Transparent = true;
			    }

			    CachedBlockMeta.TryAdd(id, meta);
		    }
		}

	    private static BlockModel GetOrCacheModel(uint state, ResourceManager resources, McResourcePack resourcePack,
			 Func<ResourceManager, BlockModel> variant)
	    {
		    if (ModelCache.TryGetValue(state, out var r))
		    {
			    return r;
			}
		    else
		    {
			    var v = variant.Invoke(resources);

				ModelCache.TryAdd(state, v);
			    return v;
		    }
		    //return _modelCache.GetOrAdd(state, u => variant.Invoke(resources));
	    }

	    internal static int LoadResources(ResourceManager resources, McResourcePack resourcePack, bool replace,
		    bool reportMissing = false)
	    {
		    if (resourcePack.TryGetBlockModel("cube_all", out ResourcePackLib.Json.Models.BlockModel cube))
		    {
			    cube.Textures["all"] = "no_texture";
			    CubeModel = cube;
		    }

		    Dictionary<uint, string> blockStateIds =
			    JsonConvert.DeserializeObject<Dictionary<uint, string>>(Encoding.UTF8.GetString(Resources.blockstate_ids));

		    int importCounter = 0;
		    foreach (var blockState in blockStateIds)
		    {
			    var id = blockState.Key;

			    int blockId = (int) (id >> 4);
			    byte metadata = (byte) (id & 0x0F);

			    string variantKey;
			    var result = Parse(resourcePack, blockState.Value, out variantKey);
			    if (result == null)
			    {
				    if (reportMissing && !IsRegistered(blockId, metadata))
					    Log.Warn($"Missing blockstate for {blockState.Value} (ID: {blockId} Meta: {metadata})");

				    continue;
			    }
			    /*if (result )
			    {
				    Log.Warn($"Missing blockstate model for {blockState.Value} (ID: {blockID} Meta: {metadata})");
				    continue;
			    }*/

			    var cached = GetOrCacheModel(id, resources, resourcePack, result);

			    if (Load(id, () =>
			    {
				    var block = new Block(id);

				    BlockMeta knownMeta;
				    if (!CachedBlockMeta.TryGetValue(blockId, out knownMeta))
				    {
					    knownMeta = new BlockMeta
					    {
						    Transparent = false,
						    DisplayName = blockState.Value
					    };
				    }

				    IBlockState blockStateData = null;

				    var split1 = blockState.Value.Split('[', ']');
				    if (split1.Length > 1)
				    {
					    blockStateData = Blocks.State.BlockState.FromString(split1[1]);
					}
				    else
				    {
						blockStateData = new Blocks.State.BlockState();
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

				 //   if (blockId == 8 || blockId == 9 || blockId == 10 || blockId == 11)
				//    {
				//	    block.Solid = false;
				//    }

				    if (blockStateData != null)
					{
						blockStateData.SetBlock(block);
						RegisteredBlockStates.TryAdd((int) id, blockStateData);
				    }

				  //  foreach (var solid in knownMeta.IsSideSolid)
				  //  {
					//    block.SetSideSolid(solid.Key, solid.Value);
				   // }

				    return block;
			    }, replace))
			    {
				    importCounter++;
			    }
		    }

		 //   for (int level = 0; level < 8; level++)
		  //  {
		//		Block.GetBlockStateID(8, )
		  //  }

		    return importCounter;
	    }

	    private static bool Load(uint id, Func<Block> blockFunction, bool replace)
	    {
		    if (replace)
		    {
			    if (RegisteredBlocks.ContainsKey(id))
			    {
				    RegisteredBlocks[id] = blockFunction();
			    }
			    else
			    {
					RegisteredBlocks.Add(id, blockFunction());
			    }
			   // _registeredBlocks.AddOrUpdate(id, blockFunction, (u, func) => { return blockFunction; });
			    return true;
		    }

		    return RegisteredBlocks.TryAdd(id, blockFunction());
	    }

		private static Func<ResourceManager, BlockModel> Parse(McResourcePack resources, string rawBlockState, out string variantKey)
	    {
		    variantKey = string.Empty;

		    Dictionary<string, string> data = null;

			var split1 = rawBlockState.Split('[', ']');
		    string name = split1[0].Replace("minecraft:", "");
		    int level = 8;
			if (split1.Length > 1)
		    {
				data = ParseData(split1[1]);

			    string color = null;
				data.TryGetValue("color", out color);

			    string variant = null;
			    data.TryGetValue("variant", out variant);

			    string type = null;
			    data.TryGetValue("type", out type);

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
			}

			if (name.Equals("water", StringComparison.InvariantCultureIgnoreCase))
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
			}

			if (resources.BlockStates.TryGetValue(name, out BlockState blockState))
		    {
			    if (blockState != null && blockState.Variants != null && blockState.Variants.Count > 0)
			    {
				    if (blockState.Variants.Count == 1)
				    {
					    var v = blockState.Variants.FirstOrDefault();
					    variantKey = v.Key;
					    return r => new CachedResourcePackModel(r, new[] { v.Value.FirstOrDefault() }) ;
				    }

				    BlockStateVariant variant = null;

				    if (split1.Length > 1)
				    {
					    int closestMatch = int.MinValue;
					    KeyValuePair<string, BlockStateVariant> closest = default(KeyValuePair<string, BlockStateVariant>);
					    foreach (var v in blockState.Variants)
					    {
						    Dictionary<string, string> variantData = ParseData(v.Key);
						    int matches = 0;
						    foreach (var kv in data)
						    {
							    if (variantData.ContainsKey(kv.Key))
							    {
								    if (variantData[kv.Key].Equals(kv.Value, StringComparison.InvariantCultureIgnoreCase))
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

					    variantKey = closest.Key;
					    variant = closest.Value;
				    }


				    if (variant == null)
				    {
					    var a = blockState.Variants.FirstOrDefault();
					    variant = a.Value;
					    variantKey = a.Key;
				    }
					
				    
				    var subVariant = variant.FirstOrDefault();
				    return r => new CachedResourcePackModel(r, new[] { subVariant });
				//	return new BlockStateModel[]{ subVariant};
			    }

			    if (blockState != null && blockState.Parts != null && blockState.Parts.Length > 0)
			    {
				    return m => new MultiStateResourcePackModel(m, blockState);
			    }
		    }

			variantKey = string.Empty;
			return null;
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

	    private static bool IsRegistered(int blockId, byte meta)
	    {
		    return RegisteredBlocks.ContainsKey(Block.GetBlockStateID(blockId, meta));
	    }
		
		private static readonly IBlockState AirState = new Blocks.State.BlockState();
	    public static IBlockState GetBlockState(int palleteId)
	    {
		    if (RegisteredBlockStates.TryGetValue(palleteId, out var result))
		    {
			    return result;
		    }

		    return AirState;
	    }

	    public static int GetBlockStateId(IBlockState state)
	    {
		    var first = RegisteredBlockStates.FirstOrDefault(x => x.Value == state).Key;

		    return first;

	    }

	    public static Block GetBlock(uint palleteId)
	    {
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

	    public static Block GetBlock(short id, byte metadata)
		{
			return GetBlock(Block.GetBlockStateID(id, metadata));
		}

	    private class BlockMeta
	    {
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
