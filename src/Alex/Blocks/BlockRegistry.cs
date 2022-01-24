using System;
using System.IO;
using System.Linq;
using System.Text;
using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Minecraft.Buttons;
using Alex.Blocks.Minecraft.Decorations;
using Alex.Blocks.Minecraft.Doors;
using Alex.Blocks.Minecraft.Fences;
using Alex.Blocks.Minecraft.Leaves;
using Alex.Blocks.Minecraft.Liquid;
using Alex.Blocks.Minecraft.Logs;
using Alex.Blocks.Minecraft.Planks;
using Alex.Blocks.Minecraft.Saplings;
using Alex.Blocks.Minecraft.Slabs;
using Alex.Blocks.Minecraft.Stairs;
using Alex.Blocks.Minecraft.Terracotta;
using Alex.Blocks.Minecraft.Walls;
using Alex.Common.Resources;
using Alex.Entities.BlockEntities;
using Alex.Utils;
using NLog;

namespace Alex.Blocks
{
	public class BlockRegistryEntry : IRegistryEntry<Block>
	{
		private readonly Func<Block> _factory;

		/// <inheritdoc />
		public ResourceLocation Location { get; private set; }

		public BlockRegistryEntry(Func<Block> factory)
		{
			_factory = factory;
		}

		/// <inheritdoc />
		public IRegistryEntry<Block> WithLocation(ResourceLocation location)
		{
			Location = location;

			return this;
		}

		/// <inheritdoc />
		public Block Value
		{
			get
			{
				var block = _factory();

				return block.WithLocation(Location).Value;
			}
		}
	}

	public class BlockRegistry : RegistryBase<Block>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockRegistry));

		private void RegisterBlock(string block, Func<Block> factory)
		{
			this.Register(block, new BlockRegistryEntry(factory));
		}

		private void RegisterBlockRange(params Func<IRegistryEntry<Block>>[] factories)
		{
			foreach (var factory in factories)
			{
				var value = factory();
				this.Register(value.Location, new BlockRegistryEntry(() => { return factory().Value; }));
			}
		}

		public BlockRegistry() : base("block")
		{
			RegisterBlock("minecraft:air", () => new Air());
			RegisterBlock("minecraft:cave_air", () => new Air());
			RegisterBlock("minecraft:void_air", () => new Air());

			RegisterBlock("minecraft:stone", () => new Stone());
			RegisterBlock("minecraft:dirt", () => new Dirt());
			RegisterBlock("minecraft:podzol", () => new Podzol());
			RegisterBlock("minecraft:cobblestone", () => new Cobblestone());
			RegisterBlock("minecraft:bedrock", () => new Bedrock());
			RegisterBlock("minecraft:sand", () => new Sand());
			RegisterBlock("minecraft:gravel", () => new Gravel());
			RegisterBlock("minecraft:sponge", () => new Sponge());
			RegisterBlock("minecraft:glass", () => new Glass());
			RegisterBlock("minecraft:dispenser", () => new Dispenser());
			RegisterBlock("minecraft:sandstone", () => new Sandstone());
			RegisterBlock("minecraft:note_block", () => new NoteBlock());
			RegisterBlock("minecraft:detector_rail", () => new DetectorRail());
			RegisterBlock("minecraft:grass", () => new Grass());
			RegisterBlock("minecraft:fern", () => new Fern());
			RegisterBlock("minecraft:large_fern", () => new Fern());
			RegisterBlock("minecraft:brown_mushroom", () => new BrownMushroom());
			RegisterBlock("minecraft:red_mushroom", () => new RedMushroom());
			RegisterBlock("minecraft:dead_bush", () => new DeadBush());
			RegisterBlock("minecraft:tnt", () => new Tnt());
			RegisterBlock("minecraft:bookshelf", () => new Bookshelf());
			RegisterBlock("minecraft:mossy_cobblestone", () => new MossyCobblestone());
			RegisterBlock("minecraft:obsidian", () => new Obsidian());
			RegisterBlock("minecraft:fire", () => new Fire());
			RegisterBlock("minecraft:mob_spawner", () => new MobSpawner());
			RegisterBlock("minecraft:spawner", () => new MobSpawner());

			RegisterBlock("minecraft:crafting_table", () => new CraftingTable());
			RegisterBlock("minecraft:wheat", () => new Wheat());
			RegisterBlock("minecraft:farmland", () => new Farmland());
			RegisterBlock("minecraft:furnace", () => new Furnace());
			RegisterBlock("minecraft:ladder", () => new Ladder());
			RegisterBlock("minecraft:rail", () => new Rail());
			RegisterBlock("minecraft:snow", () => new Snow());
			RegisterBlock("minecraft:snow_block", () => new SnowBlock());
			RegisterBlock("minecraft:ice", () => new Ice());
			RegisterBlock("minecraft:blue_ice", () => new BlueIce());
			RegisterBlock("minecraft:cactus", () => new Cactus());
			RegisterBlock("minecraft:clay", () => new Clay());
			RegisterBlock("minecraft:pumpkin", () => new Pumpkin());
			RegisterBlock("minecraft:netherrack", () => new Netherrack());
			RegisterBlock("minecraft:soul_sand", () => new SoulSand());
			RegisterBlock("minecraft:glowstone", () => new Glowstone());
			RegisterBlock("minecraft:portal", () => new Portal());
			RegisterBlock("minecraft:nether_portal", () => new Portal());
			RegisterBlock("minecraft:cake", () => new Cake());
			RegisterBlock("minecraft:brown_mushroom_block", () => new BrownMushroomBlock());
			RegisterBlock("minecraft:red_mushroom_block", () => new RedMushroomBlock());
			RegisterBlock("minecraft:iron_bars", () => new IronBars());
			RegisterBlock("minecraft:vine", () => new Vine());
			RegisterBlock("minecraft:mycelium", () => new Mycelium());
			RegisterBlock("minecraft:nether_wart", () => new NetherWart());
			RegisterBlock("minecraft:enchanting_table", () => new EnchantingTable());
			RegisterBlock("minecraft:brewing_stand", () => new BrewingStand());
			RegisterBlock("minecraft:cauldron", () => new Cauldron());
			RegisterBlock("minecraft:end_portal", () => new EndPortal());
			RegisterBlock("minecraft:end_portal_frame", () => new EndPortalFrame());
			RegisterBlock("minecraft:end_stone", () => new EndStone());
			RegisterBlock("minecraft:dragon_egg", () => new DragonEgg());
			RegisterBlock("minecraft:redstone_lamp", () => new RedstoneLamp());
			RegisterBlock("minecraft:cocoa", () => new Cocoa());

			RegisterBlock("minecraft:tripwire_hook", () => new TripwireHook());
			RegisterBlock("minecraft:tripwire", () => new Tripwire());
			RegisterBlock("minecraft:beacon", () => new Beacon());
			RegisterBlock("minecraft:carrots", () => new Carrots());
			RegisterBlock("minecraft:potatoes", () => new Potatoes());
			RegisterBlock("minecraft:anvil", () => new Anvil());
			RegisterBlock("minecraft:chipped_anvil", () => new Anvil());

			RegisterBlock("minecraft:quartz_block", () => new QuartzBlock());
			RegisterBlock("minecraft:activator_rail", () => new ActivatorRail());
			RegisterBlock("minecraft:dropper", () => new Dropper());
			RegisterBlock("minecraft:prismarine", () => new Prismarine());
			RegisterBlock("minecraft:sea_lantern", () => new SeaLantern());
			RegisterBlock("minecraft:hay_block", () => new HayBlock());
			RegisterBlock("minecraft:coal_block", () => new CoalBlock());
			RegisterBlock("minecraft:packed_ice", () => new PackedIce());
			RegisterBlock("minecraft:tall_grass", () => new TallGrass());
			RegisterBlock("minecraft:red_sandstone", () => new RedSandstone());
			RegisterBlock("minecraft:end_rod", () => new EndRod());
			RegisterBlock("minecraft:chorus_plant", () => new ChorusPlant());
			RegisterBlock("minecraft:chorus_flower", () => new ChorusFlower());
			RegisterBlock("minecraft:purpur_block", () => new PurpurBlock());
			RegisterBlock("minecraft:end_gateway", () => new EndGateway());
			RegisterBlock("minecraft:frosted_ice", () => new FrostedIce());
			RegisterBlock("minecraft:observer", () => new Observer());
			RegisterBlock("minecraft:grass_block", () => new GrassBlock());
			RegisterBlock("minecraft:powered_rail", () => new PoweredRail());
			RegisterBlock("minecraft:bricks", () => new Bricks());
			RegisterBlock("minecraft:cobweb", () => new Cobweb());
			RegisterBlock("minecraft:dandelion", () => new Dandelion());
			RegisterBlock("minecraft:poppy", () => new Poppy());
			RegisterBlock("minecraft:sugar_cane", () => new SugarCane());
			RegisterBlock("minecraft:beetroots", () => new Beetroots());
			RegisterBlock("minecraft:nether_wart_block", () => new NetherWartBlock());
			RegisterBlock("minecraft:jukebox", () => new Jukebox());
			RegisterBlock("minecraft:stone_bricks", () => new StoneBricks());
			RegisterBlock("minecraft:flower_pot", () => new FlowerPot());

			RegisterBlock("minecraft:command_block", () => new CommandBlock());
			RegisterBlock("minecraft:nether_quartz_ore", () => new NetherQuartzOre());
			RegisterBlock("minecraft:slime_block", () => new SlimeBlock());
			RegisterBlock("minecraft:purpur_pillar", () => new PurpurPillar());
			RegisterBlock("minecraft:end_stone_bricks", () => new EndStoneBricks());
			RegisterBlock("minecraft:repeating_command_block", () => new RepeatingCommandBlock());
			RegisterBlock("minecraft:chain_command_block", () => new ChainCommandBlock());
			RegisterBlock("minecraft:magma_block", () => new MagmaBlock());
			RegisterBlock("minecraft:bone_block", () => new BoneBlock());
			RegisterBlock("minecraft:structure_block", () => new StructureBlock());

			//Walls
			RegisterBlock("minecraft:cobblestone_wall", () => new CobblestoneWall());
			RegisterBlock("minecraft:mossy_cobblestone_wall", () => new CobblestoneWall());
			RegisterBlock("minecraft:andesite_wall", () => new AndesiteWall());
			RegisterBlock("minecraft:stone_brick_wall", () => new StoneBrickWall());
			RegisterBlock("minecraft:red_nether_brick_wall", () => new RedNetherBrickWall());

			RegisterBlock(
				"minecraft:sandstone_wall",
				() => new StoneBrickWall() { BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Sand) });

			//Redstone
			RegisterBlock("minecraft:lever", () => new Lever());
			RegisterBlock("minecraft:redstone_wire", () => new RedstoneWire());
			RegisterBlock("minecraft:piston", () => new Piston());
			RegisterBlock("minecraft:piston_head", () => new PistonHead());
			RegisterBlock("minecraft:sticky_piston", () => new StickyPiston());
			RegisterBlock("minecraft:daylight_detector", () => new DaylightDetector());
			RegisterBlock("minecraft:redstone_block", () => new RedstoneBlock());
			RegisterBlock("minecraft:hopper", () => new Hopper());
			RegisterBlock("minecraft:torch", () => new Torch());
			RegisterBlock("minecraft:wall_torch", () => new Torch(true));
			RegisterBlock("minecraft:redstone_torch", () => new RedstoneTorch());
			RegisterBlock("minecraft:redstone_wall_torch", () => new RedstoneTorch(true));
			RegisterBlock("minecraft:repeater", () => new Repeater());

			//Pressure plates
			RegisterBlock("minecraft:light_weighted_pressure_plate", () => new LightWeightedPressurePlate());
			RegisterBlock("minecraft:heavy_weighted_pressure_plate", () => new HeavyWeightedPressurePlate());
			RegisterBlock("minecraft:stone_pressure_plate", () => new StonePressurePlate());
			RegisterBlock("minecraft:polished_blackstone_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:oak_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:spruce_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:birch_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:jungle_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:acacia_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:dark_oak_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:warped_pressure_plate", () => new PressurePlate());
			RegisterBlock("minecraft:crimson_pressure_plate", () => new PressurePlate());

			//Buttons
			RegisterBlock("minecraft:stone_button", () => new StoneButton());
			RegisterBlock("minecraft:oak_button", () => new OakButton());
			RegisterBlock("minecraft:spruce_button", () => new SpruceButton());
			RegisterBlock("minecraft:birch_button", () => new BirchButton());
			RegisterBlock("minecraft:jungle_button", () => new JungleButton());
			RegisterBlock("minecraft:acacia_button", () => new AcaciaButton());
			RegisterBlock("minecraft:dark_oak_button", () => new DarkOakButton());
			RegisterBlock("minecraft:polished_blackstone_button", () => new PolishedBlackStoneButton());

			//Glazed Terracotta
			RegisterBlock("minecraft:white_glazed_terracotta", () => new WhiteGlazedTerracotta());
			RegisterBlock("minecraft:orange_glazed_terracotta", () => new OrangeGlazedTerracotta());
			RegisterBlock("minecraft:magenta_glazed_terracotta", () => new MagentaGlazedTerracotta());
			RegisterBlock("minecraft:light_blue_glazed_terracotta", () => new LightBlueGlazedTerracotta());
			RegisterBlock("minecraft:yellow_glazed_terracotta", () => new YellowGlazedTerracotta());
			RegisterBlock("minecraft:lime_glazed_terracotta", () => new LimeGlazedTerracotta());
			RegisterBlock("minecraft:pink_glazed_terracotta", () => new PinkGlazedTerracotta());
			RegisterBlock("minecraft:gray_glazed_terracotta", () => new GrayGlazedTerracotta());
			RegisterBlock("minecraft:cyan_glazed_terracotta", () => new CyanGlazedTerracotta());
			RegisterBlock("minecraft:purple_glazed_terracotta", () => new PurpleGlazedTerracotta());
			RegisterBlock("minecraft:blue_glazed_terracotta", () => new BlueGlazedTerracotta());
			RegisterBlock("minecraft:brown_glazed_terracotta", () => new BrownGlazedTerracotta());
			RegisterBlock("minecraft:green_glazed_terracotta", () => new GreenGlazedTerracotta());
			RegisterBlock("minecraft:red_glazed_terracotta", () => new RedGlazedTerracotta());
			RegisterBlock("minecraft:black_glazed_terracotta", () => new BlackGlazedTerracotta());
			RegisterBlock("minecraft:light_gray_glazed_terracotta", () => new LightGrayGlazedTerracotta());

			// Terracotta
			RegisterBlock("minecraft:terracotta", () => new Terracotta(ClayColor.Brown));
			RegisterBlock("minecraft:white_terracotta", () => new Terracotta(ClayColor.White));
			RegisterBlock("minecraft:orange_terracotta", () => new Terracotta(ClayColor.Orange));
			RegisterBlock("minecraft:magenta_terracotta", () => new Terracotta(ClayColor.Magenta));
			RegisterBlock("minecraft:light_blue_terracotta", () => new Terracotta(ClayColor.LightBlue));
			RegisterBlock("minecraft:yellow_terracotta", () => new Terracotta(ClayColor.Yellow));
			RegisterBlock("minecraft:lime_terracotta", () => new Terracotta(ClayColor.Lime));
			RegisterBlock("minecraft:pink_terracotta", () => new Terracotta(ClayColor.Pink));
			RegisterBlock("minecraft:gray_terracotta", () => new Terracotta(ClayColor.Gray));
			RegisterBlock("minecraft:light_gray_terracotta", () => new Terracotta(ClayColor.Gray));
			RegisterBlock("minecraft:cyan_terracotta", () => new Terracotta(ClayColor.Cyan));
			RegisterBlock("minecraft:purple_terracotta", () => new Terracotta(ClayColor.Purple));
			RegisterBlock("minecraft:blue_terracotta", () => new Terracotta(ClayColor.Blue));
			RegisterBlock("minecraft:brown_terracotta", () => new Terracotta(ClayColor.Brown));
			RegisterBlock("minecraft:green_terracotta", () => new Terracotta(ClayColor.Green));
			RegisterBlock("minecraft:red_terracotta", () => new Terracotta(ClayColor.Red));
			RegisterBlock("minecraft:black_terracotta", () => new Terracotta(ClayColor.Black));

			//Doors
			RegisterBlock("minecraft:oak_door", () => new OakDoor());
			RegisterBlock("minecraft:spruce_door", () => new SpruceDoor());
			RegisterBlock("minecraft:birch_door", () => new BirchDoor());
			RegisterBlock("minecraft:jungle_door", () => new JungleDoor());
			RegisterBlock("minecraft:acacia_door", () => new AcaciaDoor());
			RegisterBlock("minecraft:dark_oak_door", () => new DarkOakDoor());
			RegisterBlock("minecraft:iron_door", () => new IronDoor());

			//Trapdoors
			RegisterBlock("minecraft:iron_trapdoor", () => new IronTrapdoor());
			RegisterBlock("minecraft:spruce_trapdoor", () => new Trapdoor());
			RegisterBlock("minecraft:oak_trapdoor", () => new Trapdoor());
			RegisterBlock("minecraft:warped_trapdoor", () => new Trapdoor());
			RegisterBlock("minecraft:crimson_trapdoor", () => new Trapdoor());
			RegisterBlock("minecraft:acacia_trapdoor", () => new Trapdoor());
			RegisterBlock("minecraft:birch_trapdoor", () => new Trapdoor());
			RegisterBlock("minecraft:dark_oak_trapdoor", () => new Trapdoor());
			RegisterBlock("minecraft:jungle_trapdoor", () => new Trapdoor());

			//Slabs
			RegisterBlock("minecraft:oak_slab", () => new OakSlab());
			RegisterBlock("minecraft:spruce_slab", () => new SpruceSlab());
			RegisterBlock("minecraft:birch_slab", () => new BirchSlab());
			RegisterBlock("minecraft:jungle_slab", () => new JungleSlab());
			RegisterBlock("minecraft:acacia_slab", () => new AcaciaSlab());
			RegisterBlock("minecraft:dark_oak_slab", () => new DarkOakSlab());
			RegisterBlock("minecraft:stone_slab", () => new StoneSlab());
			RegisterBlock("minecraft:smooth_stone_slab", () => new StoneSlab());
			RegisterBlock("minecraft:prismarine_slab", () => new PrismarineSlab());
			RegisterBlock("minecraft:prismarine_bricks_slab", () => new PrismarineBricksSlab());
			RegisterBlock("minecraft:prismarine_brick_slab", () => new PrismarineBricksSlab());
			RegisterBlock("minecraft:dark_prismarine_slab", () => new DarkPrismarineSlab());
			RegisterBlock("minecraft:sandstone_slab", () => new SandstoneSlab());
			RegisterBlock("minecraft:smooth_sandstone_slab", () => new SandstoneSlab());
			RegisterBlock("minecraft:petrified_oak_slab", () => new PetrifiedOakSlab());
			RegisterBlock("minecraft:cobblestone_slab", () => new CobblestoneSlab());
			RegisterBlock("minecraft:mossy_cobblestone_slab", () => new CobblestoneSlab());
			RegisterBlock("minecraft:brick_slab", () => new BrickSlab());
			RegisterBlock("minecraft:stone_brick_slab", () => new StoneBrickSlab());
			RegisterBlock("minecraft:end_stone_brick_slab", () => new StoneBrickSlab());
			RegisterBlock("minecraft:mossy_stone_brick_slab", () => new StoneBrickSlab());
			RegisterBlock("minecraft:nether_brick_slab", () => new NetherBrickSlab());
			RegisterBlock("minecraft:red_nether_brick_slab", () => new NetherBrickSlab());
			RegisterBlock("minecraft:quartz_slab", () => new QuartzSlab());
			RegisterBlock("minecraft:smooth_quartz_slab", () => new QuartzSlab());
			RegisterBlock("minecraft:red_sandstone_slab", () => new RedSandstoneSlab());
			RegisterBlock("minecraft:purpur_slab", () => new PurpurSlab());
			RegisterBlock("minecraft:polished_andesite_slab", () => new PolishedAndesiteSlab());
			RegisterBlock("minecraft:andesite_slab", () => new AndesiteSlab());
			RegisterBlock("minecraft:polished_granite_slab", () => new PolishedGraniteSlab());
			RegisterBlock("minecraft:granite_slab", () => new GraniteSlab());
			RegisterBlock("minecraft:warped_double_slab", () => new StoneSlab());
			RegisterBlock("minecraft:warped_slab", () => new StoneSlab());
			RegisterBlock("minecraft:polished_blackstone_brick_slab", () => new BrickSlab());
			RegisterBlock("minecraft:polished_blackstone_slab", () => new StoneSlab());
			// RegisterBlock("minecraft:warped_slab", () => new NetherBrickSlab());

			//Leaves
			RegisterBlock("minecraft:oak_leaves", () => new OakLeaves());
			RegisterBlock("minecraft:spruce_leaves", () => new SpruceLeaves());
			RegisterBlock("minecraft:birch_leaves", () => new BirchLeaves());
			RegisterBlock("minecraft:jungle_leaves", () => new JungleLeaves());
			RegisterBlock("minecraft:acacia_leaves", () => new AcaciaLeaves());
			RegisterBlock("minecraft:dark_oak_leaves", () => new DarkOakLeaves());

			//Logs
			RegisterBlock("minecraft:oak_log", () => new Log());
			RegisterBlock("minecraft:spruce_log", () => new Log(WoodType.Spruce));
			RegisterBlock("minecraft:birch_log", () => new BirchLog());
			RegisterBlock("minecraft:jungle_log", () => new Log(WoodType.Jungle));
			RegisterBlock("minecraft:acacia_log", () => new Log(WoodType.Acacia));
			RegisterBlock("minecraft:dark_oak_log", () => new Log(WoodType.DarkOak));
			RegisterBlock("minecraft:crimson_log", () => new Log(WoodType.Crimson));
			RegisterBlock("minecraft:warped_log", () => new Log(WoodType.Warped));

			RegisterBlock("minecraft:oak_wood", () => new Log());
			RegisterBlock("minecraft:spruce_wood", () => new Log(WoodType.Spruce));
			RegisterBlock("minecraft:birch_wood", () => new BirchLog());
			RegisterBlock("minecraft:jungle_wood", () => new Log(WoodType.Jungle));
			RegisterBlock("minecraft:acacia_wood", () => new Log(WoodType.Acacia));
			RegisterBlock("minecraft:dark_oak_wood", () => new Log(WoodType.DarkOak));
			RegisterBlock("minecraft:crimson_wood", () => new Log(WoodType.Crimson));
			RegisterBlock("minecraft:warped_wood", () => new Log(WoodType.Warped));

			//Planks
			RegisterBlock("minecraft:oak_planks", () => new Planks(WoodType.Oak));
			RegisterBlock("minecraft:spruce_planks", () => new Planks(WoodType.Spruce));
			RegisterBlock("minecraft:birch_planks", () => new BirchPlanks());
			RegisterBlock("minecraft:jungle_planks", () => new Planks(WoodType.Jungle));
			RegisterBlock("minecraft:acacia_planks", () => new Planks(WoodType.Acacia));
			RegisterBlock("minecraft:crimson_planks", () => new Planks(WoodType.Crimson));
			RegisterBlock("minecraft:warped_planks", () => new Planks(WoodType.Warped));
			RegisterBlock("minecraft:dark_oak_planks", () => new DarkOakPlanks());

			//Fences & fence gates
			RegisterBlock("minecraft:oak_fence", () => new OakFence());
			RegisterBlock("minecraft:oak_fence_gate", () => new FenceGate());
			RegisterBlock("minecraft:dark_oak_fence_gate", () => new DarkOakFenceGate());
			RegisterBlock("minecraft:dark_oak_fence", () => new Fence());
			RegisterBlock("minecraft:spruce_fence_gate", () => new SpruceFenceGate());
			RegisterBlock("minecraft:spruce_fence", () => new Fence());
			RegisterBlock("minecraft:birch_fence_gate", () => new BirchFenceGate());
			RegisterBlock("minecraft:birch_fence", () => new BirchFence());
			RegisterBlock("minecraft:jungle_fence_gate", () => new JungleFenceGate());
			RegisterBlock("minecraft:jungle_fence", () => new Fence());
			RegisterBlock("minecraft:acacia_fence_gate", () => new AcaciaFenceGate());
			RegisterBlock("minecraft:acacia_fence", () => new Fence());
			RegisterBlock("minecraft:nether_brick_fence", () => new NetherBrickFence());

			//Stairs
			RegisterBlock("minecraft:stone_stairs", () => new StoneStairs());
			RegisterBlock("minecraft:diorite_stairs", () => new StoneStairs());
			RegisterBlock("minecraft:polished_diorite_stairs", () => new StoneStairs());
			RegisterBlock("minecraft:purpur_stairs", () => new PurpurStairs());
			RegisterBlock("minecraft:cobblestone_stairs", () => new CobblestoneStairs());
			RegisterBlock("minecraft:quartz_stairs", () => new QuartzStairs());
			RegisterBlock("minecraft:smooth_quartz_stairs", () => new QuartzStairs());
			RegisterBlock("minecraft:red_sandstone_stairs", () => new RedSandstoneStairs());
			RegisterBlock("minecraft:sandstone_stairs", () => new SandstoneStairs());
			RegisterBlock("minecraft:smooth_sandstone_stairs", () => new SandstoneStairs());
			RegisterBlock("minecraft:brick_stairs", () => new BrickStairs());
			RegisterBlock("minecraft:stone_brick_stairs", () => new StoneBrickStairs());
			RegisterBlock("minecraft:end_stone_brick_stairs", () => new StoneBrickStairs());
			RegisterBlock("minecraft:mossy_stone_brick_stairs", () => new StoneBrickStairs());
			RegisterBlock("minecraft:nether_brick_stairs", () => new NetherBrickStairs());
			RegisterBlock("minecraft:red_nether_brick_stairs", () => new NetherBrickStairs());
			RegisterBlock("minecraft:acacia_stairs", () => new AcaciaStairs());
			RegisterBlock("minecraft:dark_oak_stairs", () => new DarkOakStairs());
			RegisterBlock("minecraft:spruce_stairs", () => new SpruceStairs());
			RegisterBlock("minecraft:birch_stairs", () => new BirchStairs());
			RegisterBlock("minecraft:jungle_stairs", () => new JungleStairs());
			RegisterBlock("minecraft:oak_stairs", () => new OakStairs());
			RegisterBlock("minecraft:crimson_stairs", () => new CrimsonStairs());
			RegisterBlock("minecraft:polished_andesite_stairs", () => new PolisedAndesiteStairs());
			RegisterBlock("minecraft:prismarine_stairs", () => new PrismarineStairs());
			RegisterBlock("minecraft:dark_prismarine_stairs", () => new PrismarineStairs());
			RegisterBlock("minecraft:polished_blackstone_brick_stairs", () => new BrickStairs());

			RegisterBlock("minecraft:water", () => new Water());
			RegisterBlock("minecraft:flowing_water", () => new FlowingWater());

			RegisterBlock("minecraft:lava", () => new Lava());
			RegisterBlock("minecraft:flowing_lava", () => new FlowingLava());

			RegisterBlock("minecraft:kelp", () => new Kelp());
			RegisterBlock("minecraft:kelp_plant", () => new Kelp());
			RegisterBlock("minecraft:seagrass", () => new SeaGrass());
			RegisterBlock("minecraft:tall_seagrass", () => new SeaGrass());
			RegisterBlock("minecraft:lily_pad", () => new LilyPad());
			RegisterBlock("minecraft:bubble_column", () => new BubbleColumn());

			RegisterBlock("minecraft:bamboo", () => new Bamboo());

			//Ores
			RegisterBlock("minecraft:redstone_ore", () => new RedstoneOre());
			RegisterBlock("minecraft:gold_ore", () => new GoldOre());
			RegisterBlock("minecraft:iron_ore", () => new IronOre());
			RegisterBlock("minecraft:coal_ore", () => new CoalOre());
			RegisterBlock("minecraft:diamond_ore", () => new DiamondOre());
			RegisterBlock("minecraft:emerald_ore", () => new EmeraldOre());
			RegisterBlock("minecraft:lapis_ore", () => new LapisOre());

			RegisterBlock("minecraft:gold_block", () => new GoldBlock());
			RegisterBlock("minecraft:iron_block", () => new IronBlock());
			RegisterBlock("minecraft:diamond_block", () => new DiamondBlock());
			RegisterBlock("minecraft:emerald_block", () => new EmeraldBlock());
			RegisterBlock("minecraft:lapis_block", () => new LapisBlock());

			//Flowers
			RegisterBlock("minecraft:lilac", () => new Lilac());
			RegisterBlock("minecraft:rose_bush", () => new RoseBush());
			RegisterBlock("minecraft:azure_bluet", () => new AzureBluet());
			RegisterBlock("minecraft:corn_flower", () => new CornFlower());
			RegisterBlock("minecraft:cornflower", () => new CornFlower());
			RegisterBlock("minecraft:oxeye_daisy", () => new OxeyeDaisy());
			RegisterBlock("minecraft:attached_melon_stem", () => new Stem());
			RegisterBlock("minecraft:melon_stem", () => new Stem());
			RegisterBlock("minecraft:melon_block", () => new MelonBlock());
			RegisterBlock("minecraft:pumpkin_stem", () => new PumpkinStem());
			RegisterBlock("minecraft:sunflower", () => new Sunflower());
			RegisterBlock("minecraft:red_tulip", () => new Tulip());
			RegisterBlock("minecraft:pink_tulip", () => new Tulip());
			RegisterBlock("minecraft:white_tulip", () => new Tulip());
			RegisterBlock("minecraft:orange_tulip", () => new Tulip());
			RegisterBlock("minecraft:allium", () => new Allium());
			RegisterBlock("minecraft:lily_of_the_valley", () => new Lilac());
			RegisterBlock("minecraft:blue_orchid", () => new BlueOrchid());
			RegisterBlock("minecraft:peony", () => new Peony());
			RegisterBlock("minecraft:sweet_berry_bush", () => new SweetBerryBush());

			RegisterBlock("minecraft:barrier", () => new InvisibleBedrock(false));

			//Stained glass
			RegisterBlock("minecraft:white_stained_glass", () => new StainedGlass(BlockColor.White));
			RegisterBlock("minecraft:orange_stained_glass", () => new StainedGlass(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_stained_glass", () => new StainedGlass(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_stained_glass", () => new StainedGlass(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_stained_glass", () => new StainedGlass(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_stained_glass", () => new StainedGlass(BlockColor.Lime));
			RegisterBlock("minecraft:pink_stained_glass", () => new StainedGlass(BlockColor.Pink));
			RegisterBlock("minecraft:gray_stained_glass", () => new StainedGlass(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_stained_glass", () => new StainedGlass(BlockColor.LightGray));
			RegisterBlock("minecraft:purple_stained_glass", () => new StainedGlass(BlockColor.Purple));
			RegisterBlock("minecraft:blue_stained_glass", () => new StainedGlass(BlockColor.Blue));
			RegisterBlock("minecraft:brown_stained_glass", () => new StainedGlass(BlockColor.Brown));
			RegisterBlock("minecraft:green_stained_glass", () => new StainedGlass(BlockColor.Green));
			RegisterBlock("minecraft:red_stained_glass", () => new StainedGlass(BlockColor.Red));
			RegisterBlock("minecraft:black_stained_glass", () => new StainedGlass(BlockColor.Black));
			RegisterBlock("minecraft:cyan_stained_glass", () => new StainedGlass(BlockColor.Cyan));
			RegisterBlock("minecraft:glass_pane", () => new GlassPane());

			//Stained glass panes
			RegisterBlock("minecraft:white_stained_glass_pane", () => new StainedGlassPane(BlockColor.White));
			RegisterBlock("minecraft:orange_stained_glass_pane", () => new StainedGlassPane(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_stained_glass_pane", () => new StainedGlassPane(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_stained_glass_pane", () => new StainedGlassPane(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_stained_glass_pane", () => new StainedGlassPane(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_stained_glass_pane", () => new StainedGlassPane(BlockColor.Lime));
			RegisterBlock("minecraft:pink_stained_glass_pane", () => new StainedGlassPane(BlockColor.Pink));
			RegisterBlock("minecraft:gray_stained_glass_pane", () => new StainedGlassPane(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_stained_glass_pane", () => new StainedGlassPane(BlockColor.LightGray));
			RegisterBlock("minecraft:purple_stained_glass_pane", () => new StainedGlassPane(BlockColor.Purple));
			RegisterBlock("minecraft:blue_stained_glass_pane", () => new StainedGlassPane(BlockColor.Blue));
			RegisterBlock("minecraft:brown_stained_glass_pane", () => new StainedGlassPane(BlockColor.Brown));
			RegisterBlock("minecraft:green_stained_glass_pane", () => new StainedGlassPane(BlockColor.Green));
			RegisterBlock("minecraft:red_stained_glass_pane", () => new StainedGlassPane(BlockColor.Red));
			RegisterBlock("minecraft:black_stained_glass_pane", () => new StainedGlassPane(BlockColor.Black));
			RegisterBlock("minecraft:cyan_stained_glass_pane", () => new StainedGlassPane(BlockColor.Cyan));

			RegisterBlock("minecraft:grindstone", () => new Grindstone());
			RegisterBlock("minecraft:bell", () => new Bell());

			RegisterBlock("minecraft:campfire", () => new CampFire());
			RegisterBlock("minecraft:stonecutter", () => new StoneCutter());
			RegisterBlock("minecraft:crimson_stem", () => new CrimsonStem());
			RegisterBlock("minecraft:crimson_hyphae", () => new CrimsonHyphae());
			RegisterBlock("minecraft:soul_fire", () => new SoulFire());
			RegisterBlock("minecraft:soul_campfire", () => new SoulCampfire());

			//Carpet
			RegisterBlockRange(
				() => new Carpet().WithLocation("minecraft:white_carpet"),
				() => new Carpet().WithLocation("minecraft:orange_carpet"),
				() => new Carpet().WithLocation("minecraft:magenta_carpet"),
				() => new Carpet().WithLocation("minecraft:light_blue_carpet"),
				() => new Carpet().WithLocation("minecraft:yellow_carpet"),
				() => new Carpet().WithLocation("minecraft:lime_carpet"),
				() => new Carpet().WithLocation("minecraft:pink_carpet"),
				() => new Carpet().WithLocation("minecraft:gray_carpet"),
				() => new Carpet().WithLocation("minecraft:light_gray_carpet"),
				() => new Carpet().WithLocation("minecraft:cyan_carpet"),
				() => new Carpet().WithLocation("minecraft:blue_carpet"),
				() => new Carpet().WithLocation("minecraft:purple_carpet"),
				() => new Carpet().WithLocation("minecraft:green_carpet"),
				() => new Carpet().WithLocation("minecraft:brown_carpet"),
				() => new Carpet().WithLocation("minecraft:red_carpet"),
				() => new Carpet().WithLocation("minecraft:black_carpet"));

			RegisterBlock("minecraft:light_block", () => new LightBlock());

			RegisterBlock("minecraft:soul_lantern", () => new SoulLantern());
			RegisterBlock("minecraft:shroomlight", () => new Shroomlight());
			RegisterBlock("minecraft:conduit", () => new Conduit());
			RegisterBlock("minecraft:nether_sprouts", () => new NetherSprouts());
			RegisterBlock("minecraft:twisting_vines", () => new TwistingVines());
			RegisterBlock("minecraft:twisting_vines_plant", () => new TwistingVinesPlant());
			RegisterBlock("minecraft:weeping_vines_plant", () => new WeepingVinesPlant());
			RegisterBlock("minecraft:weeping_vines", () => new WeepingVines());
			RegisterBlock("minecraft:crimson_roots", () => new CrimsonRoot());
			RegisterBlock("minecraft:crimson_fungus", () => new CrimsonFungus());
			RegisterBlock("minecraft:warped_roots", () => new WarpedRoots());
			RegisterBlock("minecraft:warped_fungus", () => new WarpedFungus());

			RegisterBlock("minecraft:lantern", () => new Lantern());
			RegisterBlock("minecraft:jack_o_lantern", () => new JackOLantern());

			RegisterBlock("minecraft:lectern", () => new Lectern());

			//Skulls
			RegisterBlock("minecraft:skeleton_skull", () => new Skull() { SkullType = SkullType.Skeleton });

			RegisterBlock(
				"minecraft:wither_skeleton_skull", () => new Skull() { SkullType = SkullType.WitherSkeleton });

			RegisterBlock("minecraft:zombie_head", () => new Skull() { SkullType = SkullType.Zombie });
			RegisterBlock("minecraft:player_head", () => new Skull() { SkullType = SkullType.Player });
			RegisterBlock("minecraft:creeper_head", () => new Skull() { SkullType = SkullType.Creeper });
			RegisterBlock("minecraft:dragon_head", () => new Skull() { SkullType = SkullType.Dragon });

			//Wall skulls
			RegisterBlock("minecraft:skeleton_wall_skull", () => new WallSkull() { SkullType = SkullType.Skeleton });

			RegisterBlock(
				"minecraft:wither_skeleton_wall_skull", () => new WallSkull() { SkullType = SkullType.WitherSkeleton });

			RegisterBlock("minecraft:zombie_wall_head", () => new WallSkull() { SkullType = SkullType.Zombie });
			RegisterBlock("minecraft:player_wall_head", () => new WallSkull() { SkullType = SkullType.Player });
			RegisterBlock("minecraft:creeper_wall_head", () => new WallSkull() { SkullType = SkullType.Creeper });
			RegisterBlock("minecraft:dragon_wall_head", () => new WallSkull() { SkullType = SkullType.Dragon });

			//Signs
			RegisterBlock("minecraft:wall_sign", () => new WallSign());
			RegisterBlock("minecraft:oak_wall_sign", () => new WallSign(WoodType.Oak));
			RegisterBlock("minecraft:spruce_wall_sign", () => new WallSign(WoodType.Spruce));
			RegisterBlock("minecraft:birch_wall_sign", () => new WallSign(WoodType.Birch));
			RegisterBlock("minecraft:jungle_wall_sign", () => new WallSign(WoodType.Jungle));
			RegisterBlock("minecraft:acacia_wall_sign", () => new WallSign(WoodType.Acacia));
			RegisterBlock("minecraft:dark_oak_wall_sign", () => new WallSign(WoodType.DarkOak));
			RegisterBlock("minecraft:crimson_wall_sign", () => new WallSign(WoodType.Crimson));
			RegisterBlock("minecraft:warped_wall_sign", () => new WallSign(WoodType.Warped));

			//Standing signs
			RegisterBlock("minecraft:standing_sign", () => new StandingSign());
			RegisterBlock("minecraft:oak_sign", () => new StandingSign(WoodType.Oak));
			RegisterBlock("minecraft:spruce_sign", () => new StandingSign(WoodType.Spruce));
			RegisterBlock("minecraft:birch_sign", () => new StandingSign(WoodType.Birch));
			RegisterBlock("minecraft:jungle_sign", () => new StandingSign(WoodType.Jungle));
			RegisterBlock("minecraft:acacia_sign", () => new StandingSign(WoodType.Acacia));
			RegisterBlock("minecraft:dark_oak_sign", () => new StandingSign(WoodType.DarkOak));
			RegisterBlock("minecraft:crimson_sign", () => new StandingSign(WoodType.Crimson));
			RegisterBlock("minecraft:warped_sign", () => new StandingSign(WoodType.Warped));

			//Chests
			RegisterBlock("minecraft:chest", () => new Chest());
			RegisterBlock("minecraft:trapped_chest", () => new TrappedChest());
			RegisterBlock("minecraft:ender_chest", () => new EnderChest());

			//Saplings
			RegisterBlock("minecraft:oak_sapling", () => new Sapling(WoodType.Oak));
			RegisterBlock("minecraft:spruce_sapling", () => new Sapling(WoodType.Spruce));
			RegisterBlock("minecraft:birch_sapling", () => new Sapling(WoodType.Birch));
			RegisterBlock("minecraft:jungle_sapling", () => new Sapling(WoodType.Jungle));
			RegisterBlock("minecraft:acacia_sapling", () => new Sapling(WoodType.Acacia));
			RegisterBlock("minecraft:dark_oak_sapling", () => new Sapling(WoodType.DarkOak));
			RegisterBlock("minecraft:crimson_sapling", () => new Sapling(WoodType.Crimson));
			RegisterBlock("minecraft:warped_sapling", () => new Sapling(WoodType.Warped));

			RegisterBlock("minecraft:grass_path", () => new GrassPath());
			RegisterBlock("minecraft:dirt_path", () => new GrassPath());

			RegisterBlock("minecraft:potted_cactus", () => new PottedCactus());
			RegisterBlock("minecraft:potted_dead_bush", () => new PottedDeadBush());
			RegisterBlock("minecraft:sea_pickle", () => new SeaPickle());

			// Banners (Standing)
			RegisterBlock("minecraft:white_banner", () => new StandingBanner(BlockColor.White));
			RegisterBlock("minecraft:orange_banner", () => new StandingBanner(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_banner", () => new StandingBanner(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_banner", () => new StandingBanner(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_banner", () => new StandingBanner(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_banner", () => new StandingBanner(BlockColor.Lime));
			RegisterBlock("minecraft:pink_banner", () => new StandingBanner(BlockColor.Pink));
			RegisterBlock("minecraft:gray_banner", () => new StandingBanner(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_banner", () => new StandingBanner(BlockColor.LightGray));
			RegisterBlock("minecraft:cyan_banner", () => new StandingBanner(BlockColor.Cyan));
			RegisterBlock("minecraft:purple_banner", () => new StandingBanner(BlockColor.Purple));
			RegisterBlock("minecraft:blue_banner", () => new StandingBanner(BlockColor.Blue));
			RegisterBlock("minecraft:brown_banner", () => new StandingBanner(BlockColor.Brown));
			RegisterBlock("minecraft:green_banner", () => new StandingBanner(BlockColor.Green));
			RegisterBlock("minecraft:red_banner", () => new StandingBanner(BlockColor.Red));
			RegisterBlock("minecraft:black_banner", () => new StandingBanner(BlockColor.Black));

			// Banners (Wall)
			RegisterBlock("minecraft:white_wall_banner", () => new WallBanner(BlockColor.White));
			RegisterBlock("minecraft:orange_wall_banner", () => new WallBanner(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_wall_banner", () => new WallBanner(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_wall_banner", () => new WallBanner(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_wall_banner", () => new WallBanner(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_wall_banner", () => new WallBanner(BlockColor.Lime));
			RegisterBlock("minecraft:pink_wall_banner", () => new WallBanner(BlockColor.Pink));
			RegisterBlock("minecraft:gray_wall_banner", () => new WallBanner(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_wall_banner", () => new WallBanner(BlockColor.LightGray));
			RegisterBlock("minecraft:cyan_wall_banner", () => new WallBanner(BlockColor.Cyan));
			RegisterBlock("minecraft:purple_wall_banner", () => new WallBanner(BlockColor.Purple));
			RegisterBlock("minecraft:blue_wall_banner", () => new WallBanner(BlockColor.Blue));
			RegisterBlock("minecraft:brown_wall_banner", () => new WallBanner(BlockColor.Brown));
			RegisterBlock("minecraft:green_wall_banner", () => new WallBanner(BlockColor.Green));
			RegisterBlock("minecraft:red_wall_banner", () => new WallBanner(BlockColor.Red));
			RegisterBlock("minecraft:black_wall_banner", () => new WallBanner(BlockColor.Black));

			//Beds (I should really implement block tags...)
			RegisterBlock("minecraft:bed", () => new Bed(BlockColor.Red));
			RegisterBlock("minecraft:white_bed", () => new Bed(BlockColor.White));
			RegisterBlock("minecraft:orange_bed", () => new Bed(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_bed", () => new Bed(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_bed", () => new Bed(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_bed", () => new Bed(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_bed", () => new Bed(BlockColor.Lime));
			RegisterBlock("minecraft:pink_bed", () => new Bed(BlockColor.Pink));
			RegisterBlock("minecraft:gray_bed", () => new Bed(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_bed", () => new Bed(BlockColor.LightGray));
			RegisterBlock("minecraft:cyan_bed", () => new Bed(BlockColor.Cyan));
			RegisterBlock("minecraft:purple_bed", () => new Bed(BlockColor.Purple));
			RegisterBlock("minecraft:blue_bed", () => new Bed(BlockColor.Blue));
			RegisterBlock("minecraft:brown_bed", () => new Bed(BlockColor.Brown));
			RegisterBlock("minecraft:green_bed", () => new Bed(BlockColor.Green));
			RegisterBlock("minecraft:red_bed", () => new Bed(BlockColor.Red));
			RegisterBlock("minecraft:black_bed", () => new Bed(BlockColor.Black));

			RegisterBlock("minecraft:glow_lichen", () => new GlowLichen());
			RegisterBlock("minecraft:pointed_dripstone", () => new PointedDripstone());

			RegisterBlock("minecraft:item_frame", () => new ItemFrame());

			//Wool
			RegisterBlock("minecraft:white_wool", () => new Wool(BlockColor.White));
			RegisterBlock("minecraft:orange_wool", () => new Wool(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_wool", () => new Wool(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_wool", () => new Wool(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_wool", () => new Wool(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_wool", () => new Wool(BlockColor.Lime));
			RegisterBlock("minecraft:pink_wool", () => new Wool(BlockColor.Pink));
			RegisterBlock("minecraft:gray_wool", () => new Wool(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_wool", () => new Wool(BlockColor.LightGray));
			RegisterBlock("minecraft:cyan_wool", () => new Wool(BlockColor.Cyan));
			RegisterBlock("minecraft:purple_wool", () => new Wool(BlockColor.Purple));
			RegisterBlock("minecraft:blue_wool", () => new Wool(BlockColor.Blue));
			RegisterBlock("minecraft:brown_wool", () => new Wool(BlockColor.Brown));
			RegisterBlock("minecraft:green_wool", () => new Wool(BlockColor.Green));
			RegisterBlock("minecraft:red_wool", () => new Wool(BlockColor.Red));
			RegisterBlock("minecraft:black_wool", () => new Wool(BlockColor.Black));

			//Concrete
			RegisterBlock("minecraft:white_concrete", () => new Concrete(BlockColor.White));
			RegisterBlock("minecraft:orange_concrete", () => new Concrete(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_concrete", () => new Concrete(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_concrete", () => new Concrete(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_concrete", () => new Concrete(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_concrete", () => new Concrete(BlockColor.Lime));
			RegisterBlock("minecraft:pink_concrete", () => new Concrete(BlockColor.Pink));
			RegisterBlock("minecraft:gray_concrete", () => new Concrete(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_concrete", () => new Concrete(BlockColor.LightGray));
			RegisterBlock("minecraft:cyan_concrete", () => new Concrete(BlockColor.Cyan));
			RegisterBlock("minecraft:purple_concrete", () => new Concrete(BlockColor.Purple));
			RegisterBlock("minecraft:blue_concrete", () => new Concrete(BlockColor.Blue));
			RegisterBlock("minecraft:brown_concrete", () => new Concrete(BlockColor.Brown));
			RegisterBlock("minecraft:green_concrete", () => new Concrete(BlockColor.Green));
			RegisterBlock("minecraft:red_concrete", () => new Concrete(BlockColor.Red));
			RegisterBlock("minecraft:black_concrete", () => new Concrete(BlockColor.Black));

			//Concrete Powder
			RegisterBlock("minecraft:white_concrete_powder", () => new ConcretePowder(BlockColor.White));
			RegisterBlock("minecraft:orange_concrete_powder", () => new ConcretePowder(BlockColor.Orange));
			RegisterBlock("minecraft:magenta_concrete_powder", () => new ConcretePowder(BlockColor.Magenta));
			RegisterBlock("minecraft:light_blue_concrete_powder", () => new ConcretePowder(BlockColor.LightBlue));
			RegisterBlock("minecraft:yellow_concrete_powder", () => new ConcretePowder(BlockColor.Yellow));
			RegisterBlock("minecraft:lime_concrete_powder", () => new ConcretePowder(BlockColor.Lime));
			RegisterBlock("minecraft:pink_concrete_powder", () => new ConcretePowder(BlockColor.Pink));
			RegisterBlock("minecraft:gray_concrete_powder", () => new ConcretePowder(BlockColor.Gray));
			RegisterBlock("minecraft:light_gray_concrete_powder", () => new ConcretePowder(BlockColor.LightGray));
			RegisterBlock("minecraft:cyan_concrete_powder", () => new ConcretePowder(BlockColor.Cyan));
			RegisterBlock("minecraft:purple_concrete_powder", () => new ConcretePowder(BlockColor.Purple));
			RegisterBlock("minecraft:blue_concrete_powder", () => new ConcretePowder(BlockColor.Blue));
			RegisterBlock("minecraft:brown_concrete_powder", () => new ConcretePowder(BlockColor.Brown));
			RegisterBlock("minecraft:green_concrete_powder", () => new ConcretePowder(BlockColor.Green));
			RegisterBlock("minecraft:red_concrete_powder", () => new ConcretePowder(BlockColor.Red));
			RegisterBlock("minecraft:black_concrete_powder", () => new ConcretePowder(BlockColor.Black));

			RegisterBlock("minecraft:granite", () => new Granite());
			RegisterBlock("minecraft:polished_granite", () => new PolishedGranite());

			RegisterBlock("minecraft:basalt", () => new Basalt());
			RegisterBlock("minecraft:polished_basalt", () => new PolishedBasalt());
		}
	}
}