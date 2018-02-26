using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.CoreRT.Blocks;
using Alex.CoreRT.Graphics.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourcePackLib;
using ResourcePackLib.CoreRT;
using ResourcePackLib.CoreRT.Json.BlockStates;
using BlockModel = ResourcePackLib.CoreRT.Json.Models.BlockModel;

namespace Alex.CoreRT
{
    public static class BlockFactory
    {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(BlockFactory));
	    
		private static ConcurrentDictionary<uint, Func<Block>> _registeredBlocks = new ConcurrentDictionary<uint, Func<Block>>();
		private static ConcurrentDictionary<int, BlockMeta> _blockMeta = new ConcurrentDictionary<int, BlockMeta>();
		private static ConcurrentDictionary<uint, CachedResourcePackModel> _modelCache = new ConcurrentDictionary<uint, CachedResourcePackModel>();

		private static BlockModel CubeModel { get; set; }

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

			    BlockMeta meta = new BlockMeta()
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
			    }

			    _blockMeta.TryAdd(id, meta);
		    }
		}

	    private static CachedResourcePackModel GetOrCacheModel(uint state, ResourceManager resources, MCResourcePack resourcePack,
		    BlockStateModel variant)
	    {
		    return _modelCache.GetOrAdd(state, u => new CachedResourcePackModel(resources, variant));
	    }

	    internal static int LoadResources(ResourceManager resources, MCResourcePack resourcePack, bool replace, bool reportMissing = false)
		{
			if (resourcePack.TryGetBlockModel("cube_all", out BlockModel cube))
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

				int blockID = (int) (id >> 4);
				byte metadata = (byte) (id & 0x0F);

				string variantKey;
				var result = Parse(resourcePack, blockState.Value, out variantKey);
				if (result == null)
				{   if (reportMissing)
						Log.Warn($"Missing blockstate for {blockState.Value} (ID: {blockID} Meta: {metadata})");

					continue;
				}
				if (result.Model == null)
				{
					Log.Warn($"Missing blockstate model for {blockState.Value} (ID: {blockID} Meta: {metadata})");
					continue;
				}

				BlockMeta knownMeta;
				if (!_blockMeta.TryGetValue(blockID, out knownMeta))
				{
					knownMeta = new BlockMeta()
					{
						Transparent = false,
						DisplayName = blockState.Value
					};
				}

				var cached = GetOrCacheModel(id, resources, resourcePack, result);

				if (Load(id, () =>
				{
					var block = new Block(id)
					{
						BlockModel = cached,
						Transparent = knownMeta.Transparent,
						DisplayName = blockState.Value,
						LightValue = knownMeta.LightValue,
						AmbientOcclusionLightValue = knownMeta.AmbientOcclusionLightValue,
						LightOpacity = knownMeta.LightOpacity,

					};

					return block;
				}, replace))
				{
					importCounter++;
				}
			}

			return importCounter;
	    }

	    private static bool Load(uint id, Func<Block> blockFunction, bool replace)
	    {
		    if (replace)
		    {
			    _registeredBlocks.AddOrUpdate(id, blockFunction, (u, func) => { return blockFunction; });
			    return true;
		    }
		    else
		    {
			    return _registeredBlocks.TryAdd(id, blockFunction);
		    }
	    }

	    private static BlockStateModel Parse(MCResourcePack resources, string rawBlockState, out string variantKey)
	    {
		    variantKey = string.Empty;

		    Dictionary<string, string> data = null;

			var split1 = rawBlockState.Split('[', ']');
		    string name = split1[0].Replace("minecraft:", "");
		    if (split1.Length > 1)
		    {
				data = ParseData(split1[1]);
			    if (data.ContainsKey("color"))
			    {
				    name = $"{data["color"]}_{name}";
			    }
			}
			
		    if (resources.BlockStates.TryGetValue(name, out BlockState blockState))
		    {
			    if (blockState != null && blockState.Variants != null && blockState.Variants.Count > 0)
			    {
				    if (blockState.Variants.Count == 1)
				    {
					    var v = blockState.Variants.FirstOrDefault();
					    variantKey = v.Key;
					    return v.Value.FirstOrDefault();
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
				    return subVariant;
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
			    string key = split.Split('=')[0];
			    string value = split.Split('=')[1];

				values.Add(key, value);
		    }

		    return values;
	    }

	    public static Block GetBlock(uint palleteId)
	    {
		    int blockID = (int)(palleteId >> 4);
		    byte metadata = (byte)(palleteId & 0x0F);

			if (blockID == 0) return new Air();
			if (blockID == 8 || blockID == 9) return new Water(metadata);

			if (_registeredBlocks.TryGetValue(palleteId, out Func<Block> b))
		    {
			    return b();
		    }

		    return new Block(palleteId)
		    {
			    BlockModel = new ResourcePackModel(null, new BlockStateModel()
			    {
					Model = CubeModel,
					ModelName = CubeModel.Name,
					Y = 0,
					X = 0,
					Uvlock = false,
					Weight = 0
				}),
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
		    public int LightValue = 0;
		    public int LightOpacity = 0;
	    }
    }
}
