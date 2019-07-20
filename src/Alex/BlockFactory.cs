using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Alex.API.Blocks.State;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.BlockStates;
using Newtonsoft.Json;

namespace Alex
{
	public static class BlockFactory
	{
		private static NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger(typeof(BlockFactory));

		public static IReadOnlyDictionary<uint, IBlockState> AllBlockstates => new ReadOnlyDictionary<uint, IBlockState>(RegisteredBlockStates);
		public static IReadOnlyDictionary<string, BlockStateVariantMapper> AllBlockstatesByName => new ReadOnlyDictionary<string, BlockStateVariantMapper>(BlockStateByName);

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

		public static readonly LiquidBlockModel FlowingWaterModel = new LiquidBlockModel()
		{
			IsFlowing = true,
			IsLava = false,
			Level = 8
		};

		public static readonly LiquidBlockModel StationairyLavaModel = new LiquidBlockModel()
		{
			IsFlowing = false,
			IsLava = true,
			Level = 8
		};

		public static readonly LiquidBlockModel FlowingLavaModel = new LiquidBlockModel()
		{
			IsFlowing = true,
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
		internal static int LoadResources(ResourceManager resources, McResourcePack resourcePack, bool replace,
			bool reportMissing = false, IProgressReceiver progressReceiver = null)
        {
	        var raw = ResourceManager.ReadStringResource("Alex.Resources.runtimeidtable.json");
	        var raw2 = ResourceManager.ReadStringResource("Alex.Resources.PEBlocks.json");
	        //RuntimeIdTable = JsonConvert.DeserializeObject<Dictionary<string, TableEntry>>(raw2).Values.ToArray();
	        RuntimeIdTable = TableEntry.FromJson(raw);

            var blockEntries = resources.Registries.Blocks.Entries;

            progressReceiver?.UpdateProgress(0, "Loading block registry...");
            for(int i = 0; i < blockEntries.Count; i++)
            {
	            var kv = blockEntries.ElementAt(i);

                progressReceiver?.UpdateProgress(i * (100 / blockEntries.Count), "Loading block registry...", kv.Key);

                
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

			return LoadModels(resources, resourcePack, replace, reportMissing, progressReceiver);
		}

		private static PropertyBool WaterLoggedProperty = new PropertyBool("waterlogged");
		internal static bool GenerateClasses { get; set; } = false;
		private static BlockModel UnknownBlockModel { get; set; }
		private static int LoadModels(ResourceManager resources, McResourcePack resourcePack, bool replace,
			bool reportMissing, IProgressReceiver progressReceiver)
		{
			StringBuilder factoryBuilder = new StringBuilder();

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
				var state = new BlockState
				{
					Name = entry.Key
				};

				var def = entry.Value.States.FirstOrDefault(x => x.Default);
				if (def != null && def.Properties != null)
				{
					foreach (var property in def.Properties)
					{
						state = (BlockState) state.WithPropertyNoResolve(property.Key, property.Value, false);
					}
				}
				else
				{
					if (entry.Value.Properties != null)
					{
						foreach (var property in entry.Value.Properties)
						{
							state = (BlockState) state.WithPropertyNoResolve(property.Key,
								property.Value.FirstOrDefault(), false);
						}
					}
				}

				List<BlockState> variants = new List<BlockState>();
				foreach (var s in entry.Value.States)
				{
					var id = s.ID;

					BlockState variantState = (BlockState) (state).CloneSilent();
					variantState.ID = id;
					variantState.VariantMapper = variantMap;

					if (s.Properties != null)
					{
						foreach (var property in s.Properties)
						{
							//var prop = StateProperty.Parse(property.Key);
							variantState =
								(Blocks.State.BlockState) variantState.WithPropertyNoResolve(property.Key,
									property.Value, false);
							if (s.Default)
							{
								state = (BlockState) state.WithPropertyNoResolve(property.Key, property.Value, false);
							}
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
						var block = GetBlockByName(entry.Key);

						if (block == null)
						{
							block = new UnknownBlock(id);
							displayName = $"(Not implemented) {displayName}";

							block.Name = entry.Key;
						}
						else
						{
							block.Name = entry.Key;
						}


						if (block.IsSourceBlock && !(cachedBlockModel is MultiBlockModel) &&
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
						}

						if (variantState.GetTypedValue(WaterLoggedProperty))
						{
							block.Transparent = true;
						}

						variantState.Name = block.Name;
						variantState.Block = block;
						variantState.Model = cachedBlockModel;

						block.BlockState = variantState;
						if (string.IsNullOrWhiteSpace(block.DisplayName) ||
						    !block.DisplayName.Contains("minet", StringComparison.InvariantCultureIgnoreCase))
						{
							block.DisplayName = displayName;
						}

						if (s.Default) //This is the default variant.
						{
							variantMap._default = variantState;
							//variantState.Default = variantState;
							//variantState.Variants.AddRange(state.Variants);
							//state = variantState;
						}
						else
						{
							if (!variantMap.TryAdd(variantState))
							{
								Log.Warn(
									$"Could not add variant to variantmapper! ({variantState.ID} - {variantState.Name})");
								continue;
							}
						}

						if (!RegisteredBlockStates.TryAdd(id, variantState))
						{
							if (replace)
							{
								RegisteredBlockStates[id] = variantState;
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

					variants.Add(variantState);
				}

				if (variantMap._default == null)
				{
					variantMap._default = state;
				}

				foreach (var var in variants)
				{
					var.VariantMapper = variantMap;
				}


				if (!BlockStateByName.TryAdd(state.Name, variantMap))
				{
					if (replace)
					{
						BlockStateByName[state.Name] = variantMap;
					}
					else
					{
						Log.Warn($"Failed to add blockstate, key already exists! ({state.Name})");
					}
				}

				done++;
			}

			Log.Info($"Got {multipartBased} multi-part blockstate variants!");
			return importCounter;
		}

		public static string ToPascalCase(string original)
		{
			Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
			Regex whiteSpace = new Regex(@"(?<=\s)");
			Regex startsWithLowerCaseChar = new Regex("^[a-z]");
			Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
			Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
			Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

			// replace white spaces with undescore, then replace all invalid Characters with empty string
			var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
				// split by underscores
				.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
				// set first letter to uppercase
				.Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
				// replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
				.Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
				// set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
				.Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
				// lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
				.Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

			return string.Concat(pascalCase);
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
			/*	if (state.TryGetValue("level", out string lvl))
				{
					if (int.TryParse(lvl, out int actualLevel))
					{
						if (actualLevel < 7)
							return FlowingWaterModel;
					}
				}
				Log.Info($"WATER? {state.ToString()}");*/
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

		private static readonly IBlockState AirState = new BlockState(){Name = "Unknown", Block = new UnknownBlock(0)};

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

		//TODO: Categorize and implement

		private static Block GetBlockByName(string blockName)
		{
			var b = InternalGetBlockByName(blockName);

            var minetblock = MiNET.Blocks.BlockFactory.GetBlockByName(blockName);
			if (minetblock == null)
			{
				minetblock = MiNET.Blocks.BlockFactory.GetBlockByName(blockName.Replace("minecraft:", ""));
			}

			if (minetblock != null)
			{
				b.Hardness = minetblock.Hardness;
			}

			return b;
		}

		public static Block InternalGetBlockByName(string blockName)
		{
			blockName = blockName.ToLowerInvariant();
			if (string.IsNullOrWhiteSpace(blockName)) return null;
			else if (blockName == "minecraft:air" || blockName == "air") return new Air();
			else if (blockName == "minecraft:cave_air" || blockName == "caveair") return new Air();

			else if (blockName == "minecraft:stone" || blockName == "stone") return new Stone();
			else if (blockName == "minecraft:dirt" || blockName == "dirt") return new Dirt();
			else if (blockName == "minecraft:podzol" || blockName == "podzol") return new Podzol();
			else if (blockName == "minecraft:cobblestone" || blockName == "cobblestone") return new Cobblestone();
			else if (blockName == "minecraft:bedrock" || blockName == "bedrock") return new Bedrock();
			else if (blockName == "minecraft:sand" || blockName == "sand") return new Sand();
			else if (blockName == "minecraft:gravel" || blockName == "gravel") return new Gravel();
			else if (blockName == "minecraft:sponge" || blockName == "sponge") return new Sponge();
			else if (blockName == "minecraft:glass" || blockName == "glass") return new Glass();
			else if (blockName == "minecraft:dispenser" || blockName == "dispenser") return new Dispenser();
			else if (blockName == "minecraft:sandstone" || blockName == "sandstone") return new Sandstone();
			else if (blockName == "minecraft:note_block" || blockName == "noteblock") return new NoteBlock();
			else if (blockName == "minecraft:detector_rail" || blockName == "detectorrail") return new DetectorRail();
			else if (blockName == "minecraft:grass" || blockName == "grass") return new Grass();
			else if (blockName == "minecraft:fern" || blockName == "fern") return new Fern();
			else if (blockName == "minecraft:large_fern" || blockName == "largefern") return new Fern(); //TODO: Create large fern class
			else if (blockName == "minecraft:brown_mushroom" || blockName == "brownmushroom") return new BrownMushroom();
			else if (blockName == "minecraft:red_mushroom" || blockName == "redmushroom") return new RedMushroom();
			else if (blockName == "minecraft:dead_bush" || blockName == "deadbush") return new DeadBush();
			else if (blockName == "minecraft:piston" || blockName == "piston") return new Piston();
			else if (blockName == "minecraft:piston_head" || blockName == "pistonhead") return new PistonHead();
			else if (blockName == "minecraft:sticky_piston" || blockName == "stickypiston") return new StickyPiston();
			else if (blockName == "minecraft:tnt" || blockName == "tnt") return new Tnt();
			else if (blockName == "minecraft:bookshelf" || blockName == "bookshelf") return new Bookshelf();
			else if (blockName == "minecraft:mossy_cobblestone" || blockName == "mossycobblestone") return new MossyCobblestone();
			else if (blockName == "minecraft:obsidian" || blockName == "obsidian") return new Obsidian();
			else if (blockName == "minecraft:fire" || blockName == "fire") return new Fire();
			else if (blockName == "minecraft:mob_spawner" || blockName == "mobspawner") return new MobSpawner();
			else if (blockName == "minecraft:chest" || blockName == "chest") return new Chest();
			else if (blockName == "minecraft:redstone_wire" || blockName == "redstonewire") return new RedstoneWire();
			else if (blockName == "minecraft:crafting_table" || blockName == "craftingtable") return new CraftingTable();
			else if (blockName == "minecraft:wheat" || blockName == "wheat") return new Wheat();
			else if (blockName == "minecraft:farmland" || blockName == "farmland") return new Farmland();
			else if (blockName == "minecraft:furnace" || blockName == "furnace") return new Furnace();
			else if (blockName == "minecraft:ladder" || blockName == "ladder") return new Ladder();
			else if (blockName == "minecraft:rail" || blockName == "rail") return new Rail();
			else if (blockName == "minecraft:wall_sign" || blockName == "wallsign") return new WallSign();
			else if (blockName == "minecraft:lever" || blockName == "lever") return new Lever();
			else if (blockName == "minecraft:stone_pressure_plate" || blockName == "stonepressureplate") return new StonePressurePlate();
			else if (blockName == "minecraft:oak_pressure_plate" || blockName == "oakpressureplate") return new StonePressurePlate(); 
			else if (blockName == "minecraft:torch" || blockName == "torch") return new Torch();
			else if (blockName == "minecraft:wall_torch" || blockName == "walltorch") return new Torch(true); 
			else if (blockName == "minecraft:redstone_torch" || blockName == "redstonetorch") return new RedstoneTorch();
			else if (blockName == "minecraft:snow" || blockName == "snow") return new Snow();
			else if (blockName == "minecraft:ice" || blockName == "ice") return new Ice();
			else if (blockName == "minecraft:cactus" || blockName == "cactus") return new Cactus();
			else if (blockName == "minecraft:clay" || blockName == "clay") return new Clay();
			else if (blockName == "minecraft:pumpkin" || blockName == "pumpkin") return new Pumpkin();
			else if (blockName == "minecraft:netherrack" || blockName == "netherrack") return new Netherrack();
			else if (blockName == "minecraft:soul_sand" || blockName == "soulsand") return new SoulSand();
			else if (blockName == "minecraft:glowstone" || blockName == "glowstone") return new Glowstone();
			else if (blockName == "minecraft:portal" || blockName == "portal") return new Portal();
			else if (blockName == "minecraft:cake" || blockName == "cake") return new Cake();
			else if (blockName == "minecraft:brown_mushroom_block" || blockName == "brownmushroomblock") return new BrownMushroomBlock();
			else if (blockName == "minecraft:red_mushroom_block" || blockName == "redmushroomblock") return new RedMushroomBlock();
			else if (blockName == "minecraft:iron_bars" || blockName == "ironbars") return new IronBars();
			else if (blockName == "minecraft:glass_pane" || blockName == "glasspane") return new GlassPane();
			else if (blockName == "minecraft:melon_block" || blockName == "melonblock") return new MelonBlock();
			else if (blockName == "minecraft:pumpkin_stem" || blockName == "pumpkinstem") return new PumpkinStem();
			else if (blockName == "minecraft:melon_stem" || blockName == "melonstem") return new MelonStem();
			else if (blockName == "minecraft:vine" || blockName == "vine") return new Vine();
			else if (blockName == "minecraft:mycelium" || blockName == "mycelium") return new Mycelium();
			else if (blockName == "minecraft:nether_wart" || blockName == "netherwart") return new NetherWart();
			else if (blockName == "minecraft:enchanting_table" || blockName == "enchantingtable") return new EnchantingTable();
			else if (blockName == "minecraft:brewing_stand" || blockName == "brewingstand") return new BrewingStand();
			else if (blockName == "minecraft:cauldron" || blockName == "cauldron") return new Cauldron();
			else if (blockName == "minecraft:end_portal" || blockName == "endportal") return new EndPortal();
			else if (blockName == "minecraft:end_portal_frame" || blockName == "endportalframe") return new EndPortalFrame();
			else if (blockName == "minecraft:end_stone" || blockName == "endstone") return new EndStone();
			else if (blockName == "minecraft:dragon_egg" || blockName == "dragonegg") return new DragonEgg();
			else if (blockName == "minecraft:redstone_lamp" || blockName == "redstonelamp") return new RedstoneLamp();
			else if (blockName == "minecraft:cocoa" || blockName == "cocoa") return new Cocoa();
			else if (blockName == "minecraft:ender_chest" || blockName == "enderchest") return new EnderChest();
			else if (blockName == "minecraft:tripwire_hook" || blockName == "tripwirehook") return new TripwireHook();
			else if (blockName == "minecraft:tripwire" || blockName == "tripwire") return new Tripwire();
			else if (blockName == "minecraft:beacon" || blockName == "beacon") return new Beacon();
			else if (blockName == "minecraft:cobblestone_wall" || blockName == "cobblestonewall") return new CobblestoneWall();
			else if (blockName == "minecraft:flower_pot" || blockName == "flowerpot") return new FlowerPot();
			else if (blockName == "minecraft:carrots" || blockName == "carrots") return new Carrots();
			else if (blockName == "minecraft:potatoes" || blockName == "potatoes") return new Potatoes();
			else if (blockName == "minecraft:anvil" || blockName == "anvil") return new Anvil();
			else if (blockName == "minecraft:trapped_chest" || blockName == "trappedchest") return new TrappedChest();
			else if (blockName == "minecraft:light_weighted_pressure_plate" || blockName == "lightweightedpressureplate") return new LightWeightedPressurePlate();
			else if (blockName == "minecraft:heavy_weighted_pressure_plate" || blockName == "heavyweightedpressureplate") return new HeavyWeightedPressurePlate();
			else if (blockName == "minecraft:daylight_detector" || blockName == "daylightdetector") return new DaylightDetector();
			else if (blockName == "minecraft:redstone_block" || blockName == "redstoneblock") return new RedstoneBlock();
			else if (blockName == "minecraft:hopper" || blockName == "hopper") return new Hopper();
			else if (blockName == "minecraft:quartz_block" || blockName == "quartzblock") return new QuartzBlock();
			else if (blockName == "minecraft:activator_rail" || blockName == "activatorrail") return new ActivatorRail();
			else if (blockName == "minecraft:dropper" || blockName == "dropper") return new Dropper();
			
			else if (blockName == "minecraft:prismarine" || blockName == "prismarine") return new Prismarine();
			else if (blockName == "minecraft:sea_lantern" || blockName == "sealantern") return new SeaLantern();
			else if (blockName == "minecraft:hay_block" || blockName == "hayblock") return new HayBlock();
			else if (blockName == "minecraft:coal_block" || blockName == "coalblock") return new CoalBlock();
			else if (blockName == "minecraft:packed_ice" || blockName == "packedice") return new PackedIce();
			else if (blockName == "minecraft:tall_grass" || blockName == "tallgrass") return new TallGrass();
			else if (blockName == "minecraft:red_sandstone" || blockName == "redsandstone") return new RedSandstone();
			else if (blockName == "minecraft:end_rod" || blockName == "endrod") return new EndRod();
			else if (blockName == "minecraft:chorus_plant" || blockName == "chorusplant") return new ChorusPlant();
			else if (blockName == "minecraft:chorus_flower" || blockName == "chorusflower") return new ChorusFlower();
			else if (blockName == "minecraft:purpur_block" || blockName == "purpurblock") return new PurpurBlock();
			else if (blockName == "minecraft:grass_path" || blockName == "grasspath") return new GrassPath();
			else if (blockName == "minecraft:end_gateway" || blockName == "endgateway") return new EndGateway();
			else if (blockName == "minecraft:frosted_ice" || blockName == "frostedice") return new FrostedIce();
			else if (blockName == "minecraft:observer" || blockName == "observer") return new Observer();
			else if (blockName == "minecraft:grass_block" || blockName == "grassblock") return new GrassBlock();
			else if (blockName == "minecraft:powered_rail" || blockName == "poweredrail") return new PoweredRail();
			else if (blockName == "minecraft:bricks" || blockName == "bricks") return new Bricks();
			else if (blockName == "minecraft:cobweb" || blockName == "cobweb") return new Cobweb();
			else if (blockName == "minecraft:dandelion" || blockName == "dandelion") return new Dandelion();
			else if (blockName == "minecraft:poppy" || blockName == "poppy") return new Poppy();
			else if (blockName == "minecraft:sugar_cane" || blockName == "sugarcane") return new SugarCane();
			else if (blockName == "minecraft:beetroots" || blockName == "beetroots") return new Beetroots();
			else if (blockName == "minecraft:nether_wart_block" || blockName == "netherwartblock") return new NetherWartBlock();
			else if (blockName == "minecraft:jukebox" || blockName == "jukebox") return new Jukebox();
			else if (blockName == "minecraft:stone_bricks" || blockName == "stonebricks") return new StoneBricks();
			else if (blockName == "minecraft:lily_pad" || blockName == "lilypad") return new LilyPad();
			else if (blockName == "minecraft:command_block" || blockName == "commandblock") return new CommandBlock();
			else if (blockName == "minecraft:nether_quartz_ore" || blockName == "netherquartzore") return new NetherQuartzOre();
			else if (blockName == "minecraft:slime_block" || blockName == "slimeblock") return new SlimeBlock();
			else if (blockName == "minecraft:purpur_pillar" || blockName == "purpurpillar") return new PurpurPillar();
			else if (blockName == "minecraft:end_stone_bricks" || blockName == "endstonebricks") return new EndStoneBricks();
			else if (blockName == "minecraft:repeating_command_block" || blockName == "repeatingcommandblock") return new RepeatingCommandBlock();
			else if (blockName == "minecraft:chain_command_block" || blockName == "chaincommandblock") return new ChainCommandBlock();
			else if (blockName == "minecraft:magma_block" || blockName == "magmablock") return new MagmaBlock();
			else if (blockName == "minecraft:bone_block" || blockName == "boneblock") return new BoneBlock();
			else if (blockName == "minecraft:structure_block" || blockName == "structureblock") return new StructureBlock();

			//Buttons
			else if (blockName == "minecraft:stone_button" || blockName == "stonebutton") return new StoneButton();
			else if (blockName == "minecraft:oak_button" || blockName == "oakbutton") return new OakButton();
			else if (blockName == "minecraft:spruce_button" || blockName == "sprucebutton") return new SpruceButton();
			else if (blockName == "minecraft:birch_button" || blockName == "birchbutton") return new BirchButton();
			else if (blockName == "minecraft:jungle_button" || blockName == "junglebutton") return new JungleButton();
			else if (blockName == "minecraft:acacia_button" || blockName == "acaciabutton") return new AcaciaButton();
			else if (blockName == "minecraft:dark_oak_button" || blockName == "darkoakbutton") return new DarkOakButton();

			//Terracotta
			else if (blockName == "minecraft:white_glazed_terracotta" || blockName == "whiteglazedterracotta") return new WhiteGlazedTerracotta();
			else if (blockName == "minecraft:orange_glazed_terracotta" || blockName == "orangeglazedterracotta") return new OrangeGlazedTerracotta();
			else if (blockName == "minecraft:magenta_glazed_terracotta" || blockName == "magentaglazedterracotta") return new MagentaGlazedTerracotta();
			else if (blockName == "minecraft:light_blue_glazed_terracotta" || blockName == "lightblueglazedterracotta") return new LightBlueGlazedTerracotta();
			else if (blockName == "minecraft:yellow_glazed_terracotta" || blockName == "yellowglazedterracotta") return new YellowGlazedTerracotta();
			else if (blockName == "minecraft:lime_glazed_terracotta" || blockName == "limeglazedterracotta") return new LimeGlazedTerracotta();
			else if (blockName == "minecraft:pink_glazed_terracotta" || blockName == "pinkglazedterracotta") return new PinkGlazedTerracotta();
			else if (blockName == "minecraft:gray_glazed_terracotta" || blockName == "grayglazedterracotta") return new GrayGlazedTerracotta();
			else if (blockName == "minecraft:cyan_glazed_terracotta" || blockName == "cyanglazedterracotta") return new CyanGlazedTerracotta();
			else if (blockName == "minecraft:purple_glazed_terracotta" || blockName == "purpleglazedterracotta") return new PurpleGlazedTerracotta();
			else if (blockName == "minecraft:blue_glazed_terracotta" || blockName == "blueglazedterracotta") return new BlueGlazedTerracotta();
			else if (blockName == "minecraft:brown_glazed_terracotta" || blockName == "brownglazedterracotta") return new BrownGlazedTerracotta();
			else if (blockName == "minecraft:green_glazed_terracotta" || blockName == "greenglazedterracotta") return new GreenGlazedTerracotta();
			else if (blockName == "minecraft:red_glazed_terracotta" || blockName == "redglazedterracotta") return new RedGlazedTerracotta();
			else if (blockName == "minecraft:black_glazed_terracotta" || blockName == "blackglazedterracotta") return new BlackGlazedTerracotta();
			else if (blockName == "minecraft:light_gray_glazed_terracotta" || blockName == "lightgrayglazedterracotta") return new LightGrayGlazedTerracotta();

			//Doors
			else if (blockName == "minecraft:oak_door" || blockName == "oakdoor") return new OakDoor();
			else if (blockName == "minecraft:spruce_door" || blockName == "sprucedoor") return new SpruceDoor();
			else if (blockName == "minecraft:birch_door" || blockName == "birchdoor") return new BirchDoor();
			else if (blockName == "minecraft:jungle_door" || blockName == "jungledoor") return new JungleDoor();
			else if (blockName == "minecraft:acacia_door" || blockName == "acaciadoor") return new AcaciaDoor();
			else if (blockName == "minecraft:dark_oak_door" || blockName == "darkoakdoor") return new DarkOakDoor();
			else if (blockName == "minecraft:iron_door" || blockName == "irondoor") return new IronDoor();

			else if (blockName == "minecraft:iron_trapdoor" || blockName == "irontrapdoor") return new Trapdoor("minecraft:iron_trapdoor");
			else if (blockName == "minecraft:spruce_trapdoor" || blockName == "sprucetrapdoor") return new Trapdoor("minecraft:spruce_trapdoor");
			else if (blockName == "minecraft:oak_trapdoor" || blockName == "oaktrapdoor") return new Trapdoor("minecraft:oak_trapdoor");
			else if (blockName.EndsWith("_trapdoor")) return new Trapdoor(blockName);
			
			//Slabs
			else if (blockName == "minecraft:stone_slab" || blockName == "stoneslab") return new StoneSlab();
			else if (blockName == "minecraft:red_sandstone_slab" || blockName == "redsandstoneslab") return new RedSandstoneSlab();
			else if (blockName == "minecraft:purpur_slab" || blockName == "purpurslab") return new PurpurSlab();
			else if (blockName == "minecraft:prismarine_slab" || blockName == "prismarineslab") return new PrismarineSlab();
			else if (blockName == "minecraft:prismarine_bricks_slab" || blockName == "prismarinebricksslab") return new PrismarineBricksSlab();
			else if (blockName == "minecraft:dark_prismarine_slab" || blockName == "darkprismarineslab") return new DarkPrismarineSlab();
			else if (blockName == "minecraft:oak_slab" || blockName == "oakslab") return new OakSlab();
			else if (blockName == "minecraft:spruce_slab" || blockName == "spruceslab") return new SpruceSlab();
			else if (blockName == "minecraft:birch_slab" || blockName == "birchslab") return new BirchSlab();
			else if (blockName == "minecraft:jungle_slab" || blockName == "jungleslab") return new JungleSlab();
			else if (blockName == "minecraft:acacia_slab" || blockName == "acaciaslab") return new AcaciaSlab();
			else if (blockName == "minecraft:dark_oak_slab" || blockName == "darkoakslab") return new DarkOakSlab();
			else if (blockName == "minecraft:sandstone_slab" || blockName == "sandstoneslab") return new SandstoneSlab();
			else if (blockName == "minecraft:petrified_oak_slab" || blockName == "petrifiedoakslab") return new PetrifiedOakSlab();
			else if (blockName == "minecraft:cobblestone_slab" || blockName == "cobblestoneslab") return new CobblestoneSlab();
			else if (blockName == "minecraft:brick_slab" || blockName == "brickslab") return new BrickSlab();
			else if (blockName == "minecraft:stone_brick_slab" || blockName == "stonebrickslab") return new StoneBrickSlab();
			else if (blockName == "minecraft:nether_brick_slab" || blockName == "netherbrickslab") return new NetherBrickSlab();
			else if (blockName == "minecraft:quartz_slab" || blockName == "quartzslab") return new QuartzSlab();
			else if (blockName == "minecraft:red_sandstone_slab" || blockName == "redsandstoneslab") return new RedSandstoneSlab();
			else if (blockName == "minecraft:purpur_slab" || blockName == "purpurslab") return new PurpurSlab();

			//Leaves
			else if (blockName == "minecraft:oak_leaves" || blockName == "oakleaves") return new OakLeaves();
			else if (blockName == "minecraft:spruce_leaves" || blockName == "spruceleaves") return new SpruceLeaves();
			else if (blockName == "minecraft:birch_leaves" || blockName == "birchleaves") return new BirchLeaves();
			else if (blockName == "minecraft:jungle_leaves" || blockName == "jungleleaves") return new JungleLeaves();
			else if (blockName == "minecraft:acacia_leaves" || blockName == "acacialeaves") return new AcaciaLeaves();
			else if (blockName == "minecraft:dark_oak_leaves" || blockName == "darkoakleaves") return new DarkOakLeaves();

			//Fencing
			else if (blockName == "minecraft:nether_brick_fence" || blockName == "netherbrickfence") return new NetherBrickFence();
			else if (blockName == "minecraft:oak_fence" || blockName == "oakfence") return new OakFence();
			else if (blockName == "minecraft:oak_fence_gate" || blockName == "oakfencegate") return new FenceGate();
			else if (blockName == "minecraft:dark_oak_fence_gate" || blockName == "darkoakfencegate") return new DarkOakFenceGate();
			else if (blockName == "minecraft:dark_oak_fence" || blockName == "darkoakfence") return new Fence();
			else if (blockName == "minecraft:spruce_fence_gate" || blockName == "sprucefencegate") return new SpruceFenceGate();
			else if (blockName == "minecraft:spruce_fence" || blockName == "sprucefence") return new Fence();
			else if (blockName == "minecraft:birch_fence_gate" || blockName == "birchfencegate") return new BirchFenceGate();
			else if (blockName == "minecraft:birch_fence" || blockName == "birchfence") return new BirchFence();
			else if (blockName == "minecraft:jungle_fence_gate" || blockName == "junglefencegate") return new JungleFenceGate();
			else if (blockName == "minecraft:jungle_fence" || blockName == "junglefence") return new Fence();
			else if (blockName == "minecraft:acacia_fence_gate" || blockName == "acaciafencegate") return new AcaciaFenceGate();
			else if (blockName == "minecraft:acacia_fence" || blockName == "acaciafence") return new Fence();

			//Stairs
			else if (blockName == "minecraft:purpur_stairs" || blockName == "purpurstairs") return new PurpurStairs();
			else if (blockName == "minecraft:cobblestone_stairs" || blockName == "cobblestonestairs") return new CobblestoneStairs();
			else if (blockName == "minecraft:acacia_stairs" || blockName == "acaciastairs") return new AcaciaStairs();
			else if (blockName == "minecraft:dark_oak_stairs" || blockName == "darkoakstairs") return new DarkOakStairs();
			else if (blockName == "minecraft:quartz_stairs" || blockName == "quartzstairs") return new QuartzStairs();
			else if (blockName == "minecraft:red_sandstone_stairs" || blockName == "redsandstonestairs") return new RedSandstoneStairs();
			else if (blockName == "minecraft:spruce_stairs" || blockName == "sprucestairs") return new SpruceStairs();
			else if (blockName == "minecraft:birch_stairs" || blockName == "birchstairs") return new BirchStairs();
			else if (blockName == "minecraft:jungle_stairs" || blockName == "junglestairs") return new JungleStairs();
			else if (blockName == "minecraft:sandstone_stairs" || blockName == "sandstonestairs") return new SandstoneStairs();
			else if (blockName == "minecraft:brick_stairs" || blockName == "brickstairs") return new BrickStairs();
			else if (blockName == "minecraft:stone_brick_stairs" || blockName == "stonebrickstairs") return new StoneBrickStairs();
			else if (blockName == "minecraft:nether_brick_stairs" || blockName == "netherbrickstairs") return new NetherBrickStairs();
			else if (blockName == "minecraft:oak_stairs" || blockName == "oakstairs") return new OakStairs();

			//Liquid
			else if (blockName == "minecraft:water" || blockName == "water") return new Water();
			else if (blockName == "minecraft:lava" || blockName == "lava") return new Lava();
			else if (blockName == "minecraft:kelp" || blockName == "kelp") return new Kelp();

			//Ores
			else if (blockName == "minecraft:redstone_ore" || blockName == "redstoneore") return new RedstoneOre();
			else if (blockName == "minecraft:gold_ore" || blockName == "goldore") return new GoldOre();
			else if (blockName == "minecraft:iron_ore" || blockName == "ironore") return new IronOre();
			else if (blockName == "minecraft:coal_ore" || blockName == "coalore") return new CoalOre();
			else if (blockName == "minecraft:diamond_ore" || blockName == "diamondore") return new DiamondOre();
			else if (blockName == "minecraft:emerald_ore" || blockName == "emeraldore") return new EmeraldOre();
			else if (blockName == "minecraft:lapis_ore" || blockName == "lapisore") return new LapisOre();

			else if (blockName == "minecraft:gold_block" || blockName == "goldblock") return new GoldBlock();
			else if (blockName == "minecraft:iron_block" || blockName == "ironblock") return new IronBlock();
			else if (blockName == "minecraft:diamond_block" || blockName == "diamondblock") return new DiamondBlock();
			else if (blockName == "minecraft:emerald_block" || blockName == "emeraldblock") return new EmeraldBlock();
			else if (blockName == "minecraft:lapis_block" || blockName == "lapisblock") return new LapisBlock();

			//Plants
			else if (blockName == "minecraft:lilac" || blockName == "lilac") return new Lilac();
			else if (blockName == "minecraft:rose_bush" || blockName == "rosebush") return new RoseBush();
			else if (blockName == "minecraft:azure_bluet" || blockName == "azurebluet") return new AzureBluet();
			else if (blockName == "minecraft:corn_flower" || blockName == "cornflower") return new CornFlower();
			else if (blockName == "minecraft:oxeye_daisy" || blockName == "oxeyedaisy") return new OxeyeDaisy();
			else if (blockName == "minecraft:attached_melon_stem" || blockName == "attachedmelonstem")
				return new Stem();
			else if (blockName == "minecraft:melon_stem" || blockName == "melonstem")
				return new Stem();
			
            else if (blockName == "minecraft:barrier" || blockName == "barrier") return new InvisibleBedrock(false);

			else
			{
				var minetblock = MiNET.Blocks.BlockFactory.GetBlockByName(blockName);
				if (minetblock == null)
				{
					minetblock = MiNET.Blocks.BlockFactory.GetBlockByName(blockName.Replace("minecraft:", ""));
				}
				if (minetblock != null)
				{ 
					return new Block(minetblock.GetRuntimeId())
					{
						Solid = minetblock.IsSolid,
						Name = minetblock.Name,
						LightValue = minetblock.LightLevel,
						Transparent = minetblock.IsTransparent,
						IsReplacible = minetblock.IsReplacible,
						Drag = minetblock.FrictionFactor,
						Hardness = minetblock.Hardness,
						DisplayName = "MiNET:" + blockName
					};
				}

				return null;
			}

			//else return null;
		}

		private static Block Air { get; } = new Air();
		public static Block GetBlock(uint palleteId)
		{
			if (palleteId == 0) return Air;
			if (RegisteredBlockStates.TryGetValue(palleteId, out IBlockState b))
			{
				return (Block) b.Block;
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
			state.Block = result;
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
