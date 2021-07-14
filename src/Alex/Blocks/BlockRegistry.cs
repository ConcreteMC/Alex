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
    public class BlockRegistry : RegistryBase<Block>
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BlockRegistry));
	    public BlockRegistry() : base("block")
	    {
		    this.Register("minecraft:air", () => new Air());
		    this.Register("minecraft:cave_air", () => new Air());
		    this.Register("minecraft:void_air", () => new Air());
		    
		    this.Register("minecraft:stone", () => new Stone());
		    this.Register("minecraft:dirt", () => new Dirt());
		    this.Register("minecraft:podzol", () => new Podzol());
		    this.Register("minecraft:cobblestone", () => new Cobblestone());
		    this.Register("minecraft:bedrock", () => new Bedrock());
		    this.Register("minecraft:sand", () => new Sand());
		    this.Register("minecraft:gravel", () => new Gravel());
		    this.Register("minecraft:sponge", () => new Sponge());
		    this.Register("minecraft:glass", () => new Glass());
		    this.Register("minecraft:dispenser", () => new Dispenser());
		    this.Register("minecraft:sandstone", () => new Sandstone());
		    this.Register("minecraft:note_block", () => new NoteBlock());
		    this.Register("minecraft:detector_rail", () => new DetectorRail());
		    this.Register("minecraft:grass", () => new Grass());
		    this.Register("minecraft:fern", () => new Fern());
		    this.Register("minecraft:large_fern", () => new Fern());
		    this.Register("minecraft:brown_mushroom", () => new BrownMushroom());
		    this.Register("minecraft:red_mushroom", () => new RedMushroom());
		    this.Register("minecraft:dead_bush", () => new DeadBush());
		    this.Register("minecraft:tnt", () => new Tnt());
		    this.Register("minecraft:bookshelf", () => new Bookshelf());
		    this.Register("minecraft:mossy_cobblestone", () => new MossyCobblestone());
		    this.Register("minecraft:obsidian", () => new Obsidian());
		    this.Register("minecraft:fire", () => new Fire());
		    this.Register("minecraft:mob_spawner", () => new MobSpawner());
		    this.Register("minecraft:spawner", () => new MobSpawner());
		    
		    this.Register("minecraft:crafting_table", () => new CraftingTable());
		    this.Register("minecraft:wheat", () => new Wheat());
		    this.Register("minecraft:farmland", () => new Farmland());
		    this.Register("minecraft:furnace", () => new Furnace());
		    this.Register("minecraft:ladder", () => new Ladder());
		    this.Register("minecraft:rail", () => new Rail());
		    this.Register("minecraft:snow", () => new Snow());
		    this.Register("minecraft:snow_block", () => new SnowBlock());
		    this.Register("minecraft:ice", () => new Ice());
		    this.Register("minecraft:blue_ice", () => new BlueIce());
		    this.Register("minecraft:cactus", () => new Cactus());
		    this.Register("minecraft:clay", () => new Clay());
		    this.Register("minecraft:pumpkin", () => new Pumpkin());
		    this.Register("minecraft:netherrack", () => new Netherrack());
		    this.Register("minecraft:soul_sand", () => new SoulSand());
		    this.Register("minecraft:glowstone", () => new Glowstone());
		    this.Register("minecraft:portal", () => new Portal());
		    this.Register("minecraft:nether_portal", () => new Portal());
		    this.Register("minecraft:cake", () => new Cake());
		    this.Register("minecraft:brown_mushroom_block", () => new BrownMushroomBlock());
		    this.Register("minecraft:red_mushroom_block", () => new RedMushroomBlock());
		    this.Register("minecraft:iron_bars", () => new IronBars());
		    this.Register("minecraft:vine", () => new Vine());
		    this.Register("minecraft:mycelium", () => new Mycelium());
		    this.Register("minecraft:nether_wart", () => new NetherWart());
		    this.Register("minecraft:enchanting_table", () => new EnchantingTable());
		    this.Register("minecraft:brewing_stand", () => new BrewingStand());
		    this.Register("minecraft:cauldron", () => new Cauldron());
		    this.Register("minecraft:end_portal", () => new EndPortal());
		    this.Register("minecraft:end_portal_frame", () => new EndPortalFrame());
		    this.Register("minecraft:end_stone", () => new EndStone());
		    this.Register("minecraft:dragon_egg", () => new DragonEgg());
		    this.Register("minecraft:redstone_lamp", () => new RedstoneLamp());
		    this.Register("minecraft:cocoa", () => new Cocoa());
		    
		    this.Register("minecraft:tripwire_hook", () => new TripwireHook());
		    this.Register("minecraft:tripwire", () => new Tripwire());
		    this.Register("minecraft:beacon", () => new Beacon());
		    this.Register("minecraft:carrots", () => new Carrots());
		    this.Register("minecraft:potatoes", () => new Potatoes());
		    this.Register("minecraft:anvil", () => new Anvil());
		    this.Register("minecraft:chipped_anvil", () => new Anvil());
		    
		    this.Register("minecraft:quartz_block", () => new QuartzBlock());
		    this.Register("minecraft:activator_rail", () => new ActivatorRail());
		    this.Register("minecraft:dropper", () => new Dropper());
		    this.Register("minecraft:prismarine", () => new Prismarine());
		    this.Register("minecraft:sea_lantern", () => new SeaLantern());
		    this.Register("minecraft:hay_block", () => new HayBlock());
		    this.Register("minecraft:coal_block", () => new CoalBlock());
		    this.Register("minecraft:packed_ice", () => new PackedIce());
		    this.Register("minecraft:tall_grass", () => new TallGrass());
		    this.Register("minecraft:red_sandstone", () => new RedSandstone());
		    this.Register("minecraft:end_rod", () => new EndRod());
		    this.Register("minecraft:chorus_plant", () => new ChorusPlant());
		    this.Register("minecraft:chorus_flower", () => new ChorusFlower());
		    this.Register("minecraft:purpur_block", () => new PurpurBlock());
		    this.Register("minecraft:end_gateway", () => new EndGateway());
		    this.Register("minecraft:frosted_ice", () => new FrostedIce());
		    this.Register("minecraft:observer", () => new Observer());
		    this.Register("minecraft:grass_block", () => new GrassBlock());
		    this.Register("minecraft:powered_rail", () => new PoweredRail());
		    this.Register("minecraft:bricks", () => new Bricks());
		    this.Register("minecraft:cobweb", () => new Cobweb());
		    this.Register("minecraft:dandelion", () => new Dandelion());
		    this.Register("minecraft:poppy", () => new Poppy());
		    this.Register("minecraft:sugar_cane", () => new SugarCane());
		    this.Register("minecraft:beetroots", () => new Beetroots());
		    this.Register("minecraft:nether_wart_block", () => new NetherWartBlock());
		    this.Register("minecraft:jukebox", () => new Jukebox());
		    this.Register("minecraft:stone_bricks", () => new StoneBricks());
		    this.Register("minecraft:flower_pot", () => new FlowerPot());
		    
		    this.Register("minecraft:command_block", () => new CommandBlock());
		    this.Register("minecraft:nether_quartz_ore", () => new NetherQuartzOre());
		    this.Register("minecraft:slime_block", () => new SlimeBlock());
		    this.Register("minecraft:purpur_pillar", () => new PurpurPillar());
		    this.Register("minecraft:end_stone_bricks", () => new EndStoneBricks());
		    this.Register("minecraft:repeating_command_block", () => new RepeatingCommandBlock());
		    this.Register("minecraft:chain_command_block", () => new ChainCommandBlock());
		    this.Register("minecraft:magma_block", () => new MagmaBlock());
		    this.Register("minecraft:bone_block", () => new BoneBlock());
		    this.Register("minecraft:structure_block", () => new StructureBlock());
		    
		    //Walls
		    this.Register("minecraft:cobblestone_wall", () => new CobblestoneWall());
		    this.Register("minecraft:mossy_cobblestone_wall", () => new CobblestoneWall());
		    this.Register("minecraft:andesite_wall", () => new AndesiteWall());
		    this.Register("minecraft:stone_brick_wall", () => new StoneBrickWall());
			this.Register("minecraft:red_nether_brick_wall", () => new RedNetherBrickWall());
			this.Register("minecraft:sandstone_wall", () => new StoneBrickWall()
			{
				BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Sand)
			});
		    
		    //Redstone
		    this.Register("minecraft:lever", () => new Lever());
		    this.Register("minecraft:redstone_wire", () => new RedstoneWire());
		    this.Register("minecraft:piston", () => new Piston());
		    this.Register("minecraft:piston_head", () => new PistonHead());
		    this.Register("minecraft:sticky_piston", () => new StickyPiston());
		    this.Register("minecraft:daylight_detector", () => new DaylightDetector());
		    this.Register("minecraft:redstone_block", () => new RedstoneBlock());
		    this.Register("minecraft:hopper", () => new Hopper());
		    this.Register("minecraft:torch", () => new Torch());
		    this.Register("minecraft:wall_torch", () => new Torch(true));
		    this.Register("minecraft:redstone_torch", () => new RedstoneTorch());
		    this.Register("minecraft:redstone_wall_torch", () => new RedstoneTorch(true));
		    this.Register("minecraft:repeater", () => new Repeater());
		    
		    //Pressure plates
		    this.Register("minecraft:light_weighted_pressure_plate", () => new LightWeightedPressurePlate());
		    this.Register("minecraft:heavy_weighted_pressure_plate", () => new HeavyWeightedPressurePlate());
		    this.Register("minecraft:stone_pressure_plate", () => new StonePressurePlate());
		    this.Register("minecraft:polished_blackstone_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:oak_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:spruce_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:birch_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:jungle_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:acacia_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:dark_oak_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:warped_pressure_plate", () => new PressurePlate());
		    this.Register("minecraft:crimson_pressure_plate", () => new PressurePlate());
		    
		    //Buttons
		    this.Register("minecraft:stone_button", () => new StoneButton());
		    this.Register("minecraft:oak_button", () => new OakButton());
		    this.Register("minecraft:spruce_button", () => new SpruceButton());
		    this.Register("minecraft:birch_button", () => new BirchButton());
		    this.Register("minecraft:jungle_button", () => new JungleButton());
		    this.Register("minecraft:acacia_button", () => new AcaciaButton());
		    this.Register("minecraft:dark_oak_button", () => new DarkOakButton());
		    this.Register("minecraft:polished_blackstone_button", () => new PolishedBlackStoneButton());
		    
		    //Glazed Terracotta
		    this.Register("minecraft:white_glazed_terracotta", () => new WhiteGlazedTerracotta());
		    this.Register("minecraft:orange_glazed_terracotta", () => new OrangeGlazedTerracotta());
		    this.Register("minecraft:magenta_glazed_terracotta", () => new MagentaGlazedTerracotta());
		    this.Register("minecraft:light_blue_glazed_terracotta", () => new LightBlueGlazedTerracotta());
		    this.Register("minecraft:yellow_glazed_terracotta", () => new YellowGlazedTerracotta());
		    this.Register("minecraft:lime_glazed_terracotta", () => new LimeGlazedTerracotta());
		    this.Register("minecraft:pink_glazed_terracotta", () => new PinkGlazedTerracotta());
		    this.Register("minecraft:gray_glazed_terracotta", () => new GrayGlazedTerracotta());
		    this.Register("minecraft:cyan_glazed_terracotta", () => new CyanGlazedTerracotta());
		    this.Register("minecraft:purple_glazed_terracotta", () => new PurpleGlazedTerracotta());
		    this.Register("minecraft:blue_glazed_terracotta", () => new BlueGlazedTerracotta());
		    this.Register("minecraft:brown_glazed_terracotta", () => new BrownGlazedTerracotta());
		    this.Register("minecraft:green_glazed_terracotta", () => new GreenGlazedTerracotta());
		    this.Register("minecraft:red_glazed_terracotta", () => new RedGlazedTerracotta());
		    this.Register("minecraft:black_glazed_terracotta", () => new BlackGlazedTerracotta());
		    this.Register("minecraft:light_gray_glazed_terracotta", () => new LightGrayGlazedTerracotta());
		    
		    // Terracotta
		    this.Register("minecraft:terracotta", () => new Terracotta(ClayColor.Brown));
		    this.Register("minecraft:white_terracotta", () => new Terracotta(ClayColor.White));
		    this.Register("minecraft:orange_terracotta", () => new Terracotta(ClayColor.Orange));
		    this.Register("minecraft:magenta_terracotta", () => new Terracotta(ClayColor.Magenta));
		    this.Register("minecraft:light_blue_terracotta", () => new Terracotta(ClayColor.LightBlue));
		    this.Register("minecraft:yellow_terracotta", () => new Terracotta(ClayColor.Yellow));
		    this.Register("minecraft:lime_terracotta", () => new Terracotta(ClayColor.Lime));
		    this.Register("minecraft:pink_terracotta", () => new Terracotta(ClayColor.Pink));
		    this.Register("minecraft:gray_terracotta", () => new Terracotta(ClayColor.Gray));
		    this.Register("minecraft:light_gray_terracotta", () => new Terracotta(ClayColor.Gray));
		    this.Register("minecraft:cyan_terracotta", () => new Terracotta(ClayColor.Cyan));
		    this.Register("minecraft:purple_terracotta", () => new Terracotta(ClayColor.Purple));
		    this.Register("minecraft:blue_terracotta", () => new Terracotta(ClayColor.Blue));
		    this.Register("minecraft:brown_terracotta", () => new Terracotta(ClayColor.Brown));
		    this.Register("minecraft:green_terracotta", () => new Terracotta(ClayColor.Green));
		    this.Register("minecraft:red_terracotta", () => new Terracotta(ClayColor.Red));
		    this.Register("minecraft:black_terracotta", () => new Terracotta(ClayColor.Black));
		    
		    //Doors
		    this.Register("minecraft:oak_door", () => new OakDoor());
		    this.Register("minecraft:spruce_door", () => new SpruceDoor());
		    this.Register("minecraft:birch_door", () => new BirchDoor());
		    this.Register("minecraft:jungle_door", () => new JungleDoor());
		    this.Register("minecraft:acacia_door", () => new AcaciaDoor());
		    this.Register("minecraft:dark_oak_door", () => new DarkOakDoor());
		    this.Register("minecraft:iron_door", () => new IronDoor());
		    
		    //Trapdoors
		    this.Register("minecraft:iron_trapdoor", () => new IronTrapdoor());
		    this.Register("minecraft:spruce_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:oak_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:warped_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:crimson_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:acacia_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:birch_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:dark_oak_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:jungle_trapdoor", () => new Trapdoor());
		    
		    //Slabs
		    this.Register("minecraft:oak_slab", () => new OakSlab());
		    this.Register("minecraft:spruce_slab", () => new SpruceSlab());
		    this.Register("minecraft:birch_slab", () => new BirchSlab());
		    this.Register("minecraft:jungle_slab", () => new JungleSlab());
		    this.Register("minecraft:acacia_slab", () => new AcaciaSlab());
		    this.Register("minecraft:dark_oak_slab", () => new DarkOakSlab());
		    this.Register("minecraft:stone_slab", () => new StoneSlab());
		    this.Register("minecraft:smooth_stone_slab", () => new StoneSlab());
		    this.Register("minecraft:prismarine_slab", () => new PrismarineSlab());
		    this.Register("minecraft:prismarine_bricks_slab", () => new PrismarineBricksSlab());
		    this.Register("minecraft:prismarine_brick_slab", () => new PrismarineBricksSlab());
		    this.Register("minecraft:dark_prismarine_slab", () => new DarkPrismarineSlab());
		    this.Register("minecraft:sandstone_slab", () => new SandstoneSlab());
		    this.Register("minecraft:smooth_sandstone_slab", () => new SandstoneSlab());
		    this.Register("minecraft:petrified_oak_slab", () => new PetrifiedOakSlab());
		    this.Register("minecraft:cobblestone_slab", () => new CobblestoneSlab());
		    this.Register("minecraft:mossy_cobblestone_slab", () => new CobblestoneSlab());
		    this.Register("minecraft:brick_slab", () => new BrickSlab());
		    this.Register("minecraft:stone_brick_slab", () => new StoneBrickSlab());
		    this.Register("minecraft:end_stone_brick_slab", () => new StoneBrickSlab());
		    this.Register("minecraft:mossy_stone_brick_slab", () => new StoneBrickSlab());
		    this.Register("minecraft:nether_brick_slab", () => new NetherBrickSlab());
		    this.Register("minecraft:red_nether_brick_slab", () => new NetherBrickSlab());
		    this.Register("minecraft:quartz_slab", () => new QuartzSlab());
		    this.Register("minecraft:smooth_quartz_slab", () => new QuartzSlab());
		    this.Register("minecraft:red_sandstone_slab", () => new RedSandstoneSlab());
		    this.Register("minecraft:purpur_slab", () => new PurpurSlab());
		    this.Register("minecraft:polished_andesite_slab", () => new PolishedAndesiteSlab());
		    this.Register("minecraft:andesite_slab", () => new AndesiteSlab());
		    
		    //Leaves
		    this.Register("minecraft:oak_leaves", () => new OakLeaves());
		    this.Register("minecraft:spruce_leaves", () => new SpruceLeaves());
		    this.Register("minecraft:birch_leaves", () => new BirchLeaves());
		    this.Register("minecraft:jungle_leaves", () => new JungleLeaves());
		    this.Register("minecraft:acacia_leaves", () => new AcaciaLeaves());
		    this.Register("minecraft:dark_oak_leaves", () => new DarkOakLeaves());
		    
		    //Logs
		    this.Register("minecraft:oak_log", () => new Log());
		    this.Register("minecraft:spruce_log", () => new Log(WoodType.Spruce));
		    this.Register("minecraft:birch_log", () => new BirchLog());
		    this.Register("minecraft:jungle_log", () => new Log(WoodType.Jungle));
		    this.Register("minecraft:acacia_log", () => new Log(WoodType.Acacia));
		    this.Register("minecraft:dark_oak_log", () => new Log(WoodType.DarkOak));
		    this.Register("minecraft:crimson_log", () => new Log(WoodType.Crimson));
		    this.Register("minecraft:warped_log", () => new Log(WoodType.Warped));
		    
		    //Planks
		    this.Register("minecraft:oak_planks", () => new Planks(WoodType.Oak));
		    this.Register("minecraft:spruce_planks", () => new Planks(WoodType.Spruce));
		    this.Register("minecraft:birch_planks", new BirchPlanks());
		    this.Register("minecraft:jungle_planks", new Planks(WoodType.Jungle));
		    this.Register("minecraft:acacia_planks", new Planks(WoodType.Acacia));
		    this.Register("minecraft:crimson_planks", new Planks(WoodType.Crimson));
		    this.Register("minecraft:warped_planks", new Planks(WoodType.Warped));
		    this.Register("minecraft:dark_oak_planks", () => new DarkOakPlanks());

		    //Fences & fence gates
		    this.Register("minecraft:oak_fence", () => new OakFence());
		    this.Register("minecraft:oak_fence_gate", () => new FenceGate());
		    this.Register("minecraft:dark_oak_fence_gate", () => new DarkOakFenceGate());
		    this.Register("minecraft:dark_oak_fence", () => new Fence());
		    this.Register("minecraft:spruce_fence_gate", () => new SpruceFenceGate());
		    this.Register("minecraft:spruce_fence", () => new Fence());
		    this.Register("minecraft:birch_fence_gate", () => new BirchFenceGate());
		    this.Register("minecraft:birch_fence", () => new BirchFence());
		    this.Register("minecraft:jungle_fence_gate", () => new JungleFenceGate());
		    this.Register("minecraft:jungle_fence", () => new Fence());
		    this.Register("minecraft:acacia_fence_gate", () => new AcaciaFenceGate());
		    this.Register("minecraft:acacia_fence", () => new Fence());
		    this.Register("minecraft:nether_brick_fence", () => new NetherBrickFence());
		    
		    //Stairs
		    this.Register("minecraft:stone_stairs", () => new StoneStairs());
		    this.Register("minecraft:diorite_stairs", () => new StoneStairs());
		    this.Register("minecraft:polished_diorite_stairs", () => new StoneStairs());
		    this.Register("minecraft:purpur_stairs", () => new PurpurStairs());
		    this.Register("minecraft:cobblestone_stairs", () => new CobblestoneStairs());
		    this.Register("minecraft:quartz_stairs", () => new QuartzStairs());
		    this.Register("minecraft:smooth_quartz_stairs", () => new QuartzStairs());
		    this.Register("minecraft:red_sandstone_stairs", () => new RedSandstoneStairs());
		    this.Register("minecraft:sandstone_stairs", () => new SandstoneStairs());
		    this.Register("minecraft:smooth_sandstone_stairs", () => new SandstoneStairs());
		    this.Register("minecraft:brick_stairs", () => new BrickStairs());
		    this.Register("minecraft:stone_brick_stairs", () => new StoneBrickStairs());
		    this.Register("minecraft:end_stone_brick_stairs", () => new StoneBrickStairs());
		    this.Register("minecraft:mossy_stone_brick_stairs", () => new StoneBrickStairs());
		    this.Register("minecraft:nether_brick_stairs", () => new NetherBrickStairs());
		    this.Register("minecraft:red_nether_brick_stairs", () => new NetherBrickStairs());
		    this.Register("minecraft:acacia_stairs", () => new AcaciaStairs());
		    this.Register("minecraft:dark_oak_stairs", () => new DarkOakStairs());
		    this.Register("minecraft:spruce_stairs", () => new SpruceStairs());
		    this.Register("minecraft:birch_stairs", () => new BirchStairs());
		    this.Register("minecraft:jungle_stairs", () => new JungleStairs());
		    this.Register("minecraft:oak_stairs", () => new OakStairs());
		    this.Register("minecraft:crimson_stairs", () => new CrimsonStairs());
		    this.Register("minecraft:polished_andesite_stairs", () => new PolisedAndesiteStairs());
		    this.Register("minecraft:prismarine_stairs", () => new PrismarineStairs());
		    this.Register("minecraft:dark_prismarine_stairs", () => new PrismarineStairs());
		    
		    this.Register("minecraft:water", () => new Water());
		    this.Register("minecraft:flowing_water", () => new FlowingWater());
		    
		    this.Register("minecraft:lava", () => new Lava());
		    this.Register("minecraft:flowing_lava", () => new FlowingLava());
		    
		    this.Register("minecraft:kelp", () => new Kelp());
		    this.Register("minecraft:kelp_plant", () => new Kelp());
		    this.Register("minecraft:seagrass", () => new SeaGrass());
		    this.Register("minecraft:tall_seagrass", () => new SeaGrass());
		    this.Register("minecraft:lily_pad", () => new LilyPad());
		    this.Register("minecraft:bubble_column", () => new BubbleColumn());
		    
		    this.Register("minecraft:bamboo", () => new Bamboo());
		    
		    //Ores
		    this.Register("minecraft:redstone_ore", () => new RedstoneOre());
		    this.Register("minecraft:gold_ore", () => new GoldOre());
		    this.Register("minecraft:iron_ore", () => new IronOre());
		    this.Register("minecraft:coal_ore", () => new CoalOre());
		    this.Register("minecraft:diamond_ore", () => new DiamondOre());
		    this.Register("minecraft:emerald_ore", () => new EmeraldOre());
		    this.Register("minecraft:lapis_ore", () => new LapisOre());
		    
		    this.Register("minecraft:gold_block", () => new GoldBlock());
		    this.Register("minecraft:iron_block", () => new IronBlock());
		    this.Register("minecraft:diamond_block", () => new DiamondBlock());
		    this.Register("minecraft:emerald_block", () => new EmeraldBlock());
		    this.Register("minecraft:lapis_block", () => new LapisBlock());
		    
		    //Flowers
		    this.Register("minecraft:lilac", () => new Lilac());
		    this.Register("minecraft:rose_bush", () => new RoseBush());
		    this.Register("minecraft:azure_bluet", () => new AzureBluet());
		    this.Register("minecraft:corn_flower", () => new CornFlower());
		    this.Register("minecraft:cornflower", () => new CornFlower());
		    this.Register("minecraft:oxeye_daisy", () => new OxeyeDaisy());
		    this.Register("minecraft:attached_melon_stem", () => new Stem());
		    this.Register("minecraft:melon_stem", () => new Stem());
		    this.Register("minecraft:melon_block", () => new MelonBlock());
		    this.Register("minecraft:pumpkin_stem", () => new PumpkinStem());
		    this.Register("minecraft:sunflower", () => new Sunflower());
		    this.Register("minecraft:red_tulip", () => new Tulip());
		    this.Register("minecraft:pink_tulip", () => new Tulip());
		    this.Register("minecraft:white_tulip", () => new Tulip());
		    this.Register("minecraft:orange_tulip", () => new Tulip());
		    this.Register("minecraft:allium", () => new Allium());
		    this.Register("minecraft:lily_of_the_valley", () => new Lilac());
		    this.Register("minecraft:blue_orchid", () => new BlueOrchid());
		    this.Register("minecraft:peony", () => new Peony());
		    this.Register("minecraft:sweet_berry_bush", () => new SweetBerryBush());
		    
		    this.Register("minecraft:barrier", () => new InvisibleBedrock(false));

		    //Stained glass
		    this.Register("minecraft:white_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:orange_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:magenta_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:light_blue_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:yellow_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:lime_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:pink_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:gray_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:light_gray_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:purple_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:blue_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:brown_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:green_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:red_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:black_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:cyan_stained_glass", () => new StainedGlass());
		    this.Register("minecraft:glass_pane", () => new GlassPane());
		    
		    //Stained glass panes
		    this.Register("minecraft:white_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:orange_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:magenta_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:light_blue_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:yellow_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:lime_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:pink_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:gray_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:light_gray_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:purple_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:blue_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:brown_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:green_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:red_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:black_stained_glass_pane", () => new GlassPane());
		    this.Register("minecraft:cyan_stained_glass_pane", () => new GlassPane());
		    
		    this.Register("minecraft:grindstone", () => new Grindstone());
		    this.Register("minecraft:bell", () => new Bell());

		    this.Register("minecraft:campfire", () => new CampFire());
		    this.Register("minecraft:stonecutter", () => new StoneCutter());
		    this.Register("minecraft:crimson_stem", () => new CrimsonStem());
		    this.Register("minecraft:crimson_hyphae", () => new CrimsonHyphae());
		    this.Register("minecraft:soul_fire", () => new SoulFire());
		    this.Register("minecraft:soul_campfire", () => new SoulCampfire());
		    
		    //Carpet
		    this.RegisterRange(
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
		    
		    this.Register("minecraft:light_block", () => new LightBlock());
		    
		    this.Register("minecraft:soul_lantern", () => new SoulLantern());
		    this.Register("minecraft:shroomlight", () => new Shroomlight());
		    this.Register("minecraft:conduit", () => new Conduit());
		    this.Register("minecraft:nether_sprouts", () => new NetherSprouts());
		    this.Register("minecraft:twisting_vines", () => new TwistingVines());
		    this.Register("minecraft:twisting_vines_plant", () => new TwistingVinesPlant());
		    this.Register("minecraft:weeping_vines_plant", () => new WeepingVinesPlant());
		    this.Register("minecraft:weeping_vines", () => new WeepingVines());
		    this.Register("minecraft:crimson_roots", () => new CrimsonRoot());
		    this.Register("minecraft:crimson_fungus", () =>new CrimsonFungus());
		    this.Register("minecraft:warped_roots", () => new WarpedRoots());
		    this.Register("minecraft:warped_fungus", () => new WarpedFungus());
		    
		    this.Register("minecraft:lantern", () => new Lantern());
		    this.Register("minecraft:jack_o_lantern", () => new JackOLantern());
		    
		    this.Register("minecraft:lectern", () => new Lectern());
		    
		    //Skulls
		    this.Register("minecraft:skeleton_skull", () => new Skull()
		    {
			    SkullType = SkullType.Skeleton
		    });
		    this.Register("minecraft:wither_skeleton_skull", () => new Skull()
		    {
			    SkullType = SkullType.WitherSkeleton
		    });
		    this.Register("minecraft:zombie_head", () => new Skull()
		    {
			    SkullType = SkullType.Zombie
		    });
		    this.Register("minecraft:player_head", () => new Skull()
		    {
			    SkullType = SkullType.Player
		    });
		    this.Register("minecraft:creeper_head", () => new Skull()
		    {
			    SkullType = SkullType.Creeper
		    });
		    this.Register("minecraft:dragon_head", () => new Skull()
		    {
			    SkullType = SkullType.Dragon
		    });
		    
		    //Wall skulls
		    this.Register("minecraft:skeleton_wall_skull", () => new WallSkull()
		    {
			    SkullType = SkullType.Skeleton
		    });
		    this.Register("minecraft:wither_skeleton_wall_skull", () => new WallSkull()
		    {
			    SkullType = SkullType.WitherSkeleton
		    });
		    this.Register("minecraft:zombie_wall_head", () => new WallSkull()
		    {
			    SkullType = SkullType.Zombie
		    });
		    this.Register("minecraft:player_wall_head", () => new WallSkull()
		    {
			    SkullType = SkullType.Player
		    });
		    this.Register("minecraft:creeper_wall_head", () => new WallSkull()
		    {
			    SkullType = SkullType.Creeper
		    });
		    this.Register("minecraft:dragon_wall_head", () => new WallSkull()
		    {
			    SkullType = SkullType.Dragon
		    });
		    
		    //Signs
		    this.Register("minecraft:wall_sign", () => new WallSign());
		    this.Register("minecraft:oak_wall_sign", () => new WallSign(WoodType.Oak));
		    this.Register("minecraft:spruce_wall_sign", () => new WallSign(WoodType.Spruce));
		    this.Register("minecraft:birch_wall_sign", () => new WallSign(WoodType.Birch));
		    this.Register("minecraft:jungle_wall_sign", () => new WallSign(WoodType.Jungle));
		    this.Register("minecraft:acacia_wall_sign", () => new WallSign(WoodType.Acacia));
		    this.Register("minecraft:dark_oak_wall_sign", () => new WallSign(WoodType.DarkOak));
		    this.Register("minecraft:crimson_wall_sign", () => new WallSign(WoodType.Crimson));
		    this.Register("minecraft:warped_wall_sign", () => new WallSign(WoodType.Warped));
		    
		    //Standing signs
		    this.Register("minecraft:standing_sign", () => new StandingSign());
		    this.Register("minecraft:oak_sign", () => new StandingSign(WoodType.Oak));
		    this.Register("minecraft:spruce_sign", () => new StandingSign(WoodType.Spruce));
		    this.Register("minecraft:birch_sign", () => new StandingSign(WoodType.Birch));
		    this.Register("minecraft:jungle_sign", () => new StandingSign(WoodType.Jungle));
		    this.Register("minecraft:acacia_sign", () => new StandingSign(WoodType.Acacia));
		    this.Register("minecraft:dark_oak_sign", () => new StandingSign(WoodType.DarkOak));
		    this.Register("minecraft:crimson_sign", () => new StandingSign(WoodType.Crimson));
		    this.Register("minecraft:warped_sign", () => new StandingSign(WoodType.Warped));
		    
		    //Chests
		    this.Register("minecraft:chest", () => new Chest());
		    this.Register("minecraft:trapped_chest", () => new TrappedChest());
		    this.Register("minecraft:ender_chest", () => new EnderChest());
		    
		    //Saplings
		    this.Register("minecraft:oak_sapling", () => new Sapling(WoodType.Oak));
		    this.Register("minecraft:spruce_sapling", () => new Sapling(WoodType.Spruce));
		    this.Register("minecraft:birch_sapling", () => new Sapling(WoodType.Birch));
		    this.Register("minecraft:jungle_sapling", () => new Sapling(WoodType.Jungle));
		    this.Register("minecraft:acacia_sapling", () => new Sapling(WoodType.Acacia));
		    this.Register("minecraft:dark_oak_sapling", () => new Sapling(WoodType.DarkOak));
		    this.Register("minecraft:crimson_sapling", () => new Sapling(WoodType.Crimson));
		    this.Register("minecraft:warped_sapling", () => new Sapling(WoodType.Warped));
		    
		    this.Register("minecraft:grass_path", () => new GrassPath());
		    this.Register("minecraft:dirt_path", () => new GrassPath());
		    
		    this.Register("minecraft:potted_cactus", () => new PottedCactus());
		    this.Register("minecraft:potted_dead_bush", () => new PottedDeadBush());
		    this.Register("minecraft:sea_pickle", () => new SeaPickle());
		    
		    //Beds (I should really implement block tags...)
		    this.Register("minecraft:bed", () => new Bed(BedColor.Red));
		    this.Register("minecraft:white_bed", () => new Bed(BedColor.White));
		    this.Register("minecraft:orange_bed", () => new Bed(BedColor.Orange));
		    this.Register("minecraft:magenta_bed", () => new Bed(BedColor.Magenta));
		    this.Register("minecraft:light_blue_bed", () => new Bed(BedColor.LightBlue));
		    this.Register("minecraft:yellow_bed", () => new Bed(BedColor.Yellow));
		    this.Register("minecraft:lime_bed", () => new Bed(BedColor.Lime));
		    this.Register("minecraft:pink_bed", () => new Bed(BedColor.Pink));
		    this.Register("minecraft:gray_bed", () => new Bed(BedColor.Gray));
		    this.Register("minecraft:light_gray_bed", () => new Bed(BedColor.LightGray));
		    this.Register("minecraft:cyan_bed", () => new Bed(BedColor.Cyan));
		    this.Register("minecraft:purple_bed", () => new Bed(BedColor.Purple));
		    this.Register("minecraft:blue_bed", () => new Bed(BedColor.Blue));
		    this.Register("minecraft:brown_bed", () => new Bed(BedColor.Brown));
		    this.Register("minecraft:green_bed", () => new Bed(BedColor.Green));
		    this.Register("minecraft:red_bed", () => new Bed(BedColor.Red));
		    this.Register("minecraft:black_bed", () => new Bed(BedColor.Black));
		    
		    this.Register("minecraft:glow_lichen", () => new GlowLichen());
		    this.Register("minecraft:pointed_dripstone", () => new PointedDripstone());
		    
		    this.Register("minecraft:item_frame", () => new ItemFrame());
		    
		    //Wool
		    this.Register("minecraft:white_wool", () => new Wool(BedColor.White));
		    this.Register("minecraft:orange_wool", () => new Wool(BedColor.Orange));
		    this.Register("minecraft:magenta_wool", () => new Wool(BedColor.Magenta));
		    this.Register("minecraft:light_blue_wool", () => new Wool(BedColor.LightBlue));
		    this.Register("minecraft:yellow_wool", () => new Wool(BedColor.Yellow));
		    this.Register("minecraft:lime_wool", () => new Wool(BedColor.Lime));
		    this.Register("minecraft:pink_wool", () => new Wool(BedColor.Pink));
		    this.Register("minecraft:gray_wool", () => new Wool(BedColor.Gray));
		    this.Register("minecraft:light_gray_wool", () => new Wool(BedColor.LightGray));
		    this.Register("minecraft:cyan_wool", () => new Wool(BedColor.Cyan));
		    this.Register("minecraft:purple_wool", () => new Wool(BedColor.Purple));
		    this.Register("minecraft:blue_wool", () => new Wool(BedColor.Blue));
		    this.Register("minecraft:brown_wool", () => new Wool(BedColor.Brown));
		    this.Register("minecraft:green_wool", () => new Wool(BedColor.Green));
		    this.Register("minecraft:red_wool", () => new Wool(BedColor.Red));
		    this.Register("minecraft:black_wool", () => new Wool(BedColor.Black));
	    }
    }
}