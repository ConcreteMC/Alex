using Alex.API.Resources;
using Alex.Blocks.Minecraft;

namespace Alex.Blocks
{
    public class BlockRegistry : RegistryBase<Block>
    {
	    public BlockRegistry() : base("block")
	    {
		    this.Register("minecraft:air", () => new Air());
		    this.Register("minecraft:cave_air", () => new Air());

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
		    this.Register("minecraft:chest", () => new Chest());
		    this.Register("minecraft:crafting_table", () => new CraftingTable());
		    this.Register("minecraft:wheat", () => new Wheat());
		    this.Register("minecraft:farmland", () => new Farmland());
		    this.Register("minecraft:furnace", () => new Furnace());
		    this.Register("minecraft:ladder", () => new Ladder());
		    this.Register("minecraft:rail", () => new Rail());
		    this.Register("minecraft:wall_sign", () => new WallSign());
		    this.Register("minecraft:snow", () => new Snow());
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
		    this.Register("minecraft:ender_chest", () => new EnderChest());
		    this.Register("minecraft:tripwire_hook", () => new TripwireHook());
		    this.Register("minecraft:tripwire", () => new Tripwire());
		    this.Register("minecraft:beacon", () => new Beacon());
		    this.Register("minecraft:cobblestone_wall", () => new CobblestoneWall());
		    this.Register("minecraft:flower_pot", () => new FlowerPot());
		    this.Register("minecraft:carrots", () => new Carrots());
		    this.Register("minecraft:potatoes", () => new Potatoes());
		    this.Register("minecraft:anvil", () => new Anvil());
		    this.Register("minecraft:trapped_chest", () => new TrappedChest());
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
		    this.Register("minecraft:grass_path", () => new GrassPath());
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
		    this.Register("minecraft:lily_pad", () => new LilyPad());
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
		    
		    //Redstone
		    this.Register("minecraft:lever", () => new Lever());
		    this.Register("minecraft:redstone_wire", () => new RedstoneWire());
		    this.Register("minecraft:piston", () => new Piston());
		    this.Register("minecraft:piston_head", () => new PistonHead());
		    this.Register("minecraft:sticky_piston", () => new StickyPiston());
		    this.Register("minecraft:daylight_detector", () => new DaylightDetector());
		    this.Register("minecraft:redstone_block", () => new RedstoneBlock());
		    this.Register("minecraft:hopper", () => new Hopper());
		    this.Register("minecraft:light_weighted_pressure_plate", () => new LightWeightedPressurePlate());
		    this.Register("minecraft:heavy_weighted_pressure_plate", () => new HeavyWeightedPressurePlate());
		    this.Register("minecraft:stone_pressure_plate", () => new StonePressurePlate());
		    this.Register("minecraft:oak_pressure_plate", () => new StonePressurePlate());
		    this.Register("minecraft:torch", () => new Torch());
		    this.Register("minecraft:wall_torch", () => new Torch(true));
		    this.Register("minecraft:redstone_torch", () => new RedstoneTorch());
		    this.Register("minecraft:redstone_wall_torch", () => new RedstoneTorch(true));
		    
		    //Buttons
		    this.Register("minecraft:stone_button", () => new StoneButton());
		    this.Register("minecraft:oak_button", () => new OakButton());
		    this.Register("minecraft:spruce_button", () => new SpruceButton());
		    this.Register("minecraft:birch_button", () => new BirchButton());
		    this.Register("minecraft:jungle_button", () => new JungleButton());
		    this.Register("minecraft:acacia_button", () => new AcaciaButton());
		    this.Register("minecraft:dark_oak_button", () => new DarkOakButton());
		    
		    //Terracotta
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
		    this.Register("minecraft:white_terracotta", () => new Terracotta());
		    
		    //Doors
		    this.Register("minecraft:oak_door", () => new OakDoor());
		    this.Register("minecraft:spruce_door", () => new SpruceDoor());
		    this.Register("minecraft:birch_door", () => new BirchDoor());
		    this.Register("minecraft:jungle_door", () => new JungleDoor());
		    this.Register("minecraft:acacia_door", () => new AcaciaDoor());
		    this.Register("minecraft:dark_oak_door", () => new DarkOakDoor());
		    this.Register("minecraft:iron_door", () => new IronDoor());
		    
		    //Trapdoors
		    this.Register("minecraft:iron_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:spruce_trapdoor", () => new Trapdoor());
		    this.Register("minecraft:oak_trapdoor", () => new Trapdoor());
		    
		    //Slabs
		    this.Register("minecraft:oak_slab", () => new OakSlab());
		    this.Register("minecraft:spruce_slab", () => new SpruceSlab());
		    this.Register("minecraft:birch_slab", () => new BirchSlab());
		    this.Register("minecraft:jungle_slab", () => new JungleSlab());
		    this.Register("minecraft:acacia_slab", () => new AcaciaSlab());
		    this.Register("minecraft:dark_oak_slab", () => new DarkOakSlab());
		    this.Register("minecraft:stone_slab", () => new StoneSlab());
		    this.Register("minecraft:prismarine_slab", () => new PrismarineSlab());
		    this.Register("minecraft:prismarine_bricks_slab", () => new PrismarineBricksSlab());
		    this.Register("minecraft:dark_prismarine_slab", () => new DarkPrismarineSlab());
		    this.Register("minecraft:sandstone_slab", () => new SandstoneSlab());
		    this.Register("minecraft:petrified_oak_slab", () => new PetrifiedOakSlab());
		    this.Register("minecraft:cobblestone_slab", () => new CobblestoneSlab());
		    this.Register("minecraft:brick_slab", () => new BrickSlab());
		    this.Register("minecraft:stone_brick_slab", () => new StoneBrickSlab());
		    this.Register("minecraft:nether_brick_slab", () => new NetherBrickSlab());
		    this.Register("minecraft:quartz_slab", () => new QuartzSlab());
		    this.Register("minecraft:red_sandstone_slab", () => new RedSandstoneSlab());
		    this.Register("minecraft:purpur_slab", () => new PurpurSlab());
		    
		    //Leaves
		    this.Register("minecraft:oak_leaves", () => new OakLeaves());
		    this.Register("minecraft:spruce_leaves", () => new SpruceLeaves());
		    this.Register("minecraft:birch_leaves", () => new BirchLeaves());
		    this.Register("minecraft:jungle_leaves", () => new JungleLeaves());
		    this.Register("minecraft:acacia_leaves", () => new AcaciaLeaves());
		    this.Register("minecraft:dark_oak_leaves", () => new DarkOakLeaves());
		    
		    //Logs
		    this.Register("minecraft:oak_log", () => new Log());
		    this.Register("minecraft:acacia_log", () => new Log());
		    this.Register("minecraft:jungle_log", () => new Log());
		    this.Register("minecraft:spruce_log", () => new Log());
		    this.Register("minecraft:birch_log", () => new Log());
		    
		    //Planks
		    this.Register("minecraft:oak_planks", () => new Planks());
		    this.Register("minecraft:spruce_planks", () => new Planks());
		    this.Register("minecraft:dark_oak_planks", () => new DarkOakPlanks());
		    this.Register("minecraft:nether_brick_fence", () => new NetherBrickFence());
		    
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
		    
		    //Stairs
		    this.Register("minecraft:stone_stairs", () => new StoneStairs());
		    this.Register("minecraft:purpur_stairs", () => new PurpurStairs());
		    this.Register("minecraft:cobblestone_stairs", () => new CobblestoneStairs());
		    this.Register("minecraft:quartz_stairs", () => new QuartzStairs());
		    this.Register("minecraft:red_sandstone_stairs", () => new RedSandstoneStairs());
		    this.Register("minecraft:sandstone_stairs", () => new SandstoneStairs());
		    this.Register("minecraft:brick_stairs", () => new BrickStairs());
		    this.Register("minecraft:stone_brick_stairs", () => new StoneBrickStairs());
		    this.Register("minecraft:nether_brick_stairs", () => new NetherBrickStairs());
		    this.Register("minecraft:acacia_stairs", () => new AcaciaStairs());
		    this.Register("minecraft:dark_oak_stairs", () => new DarkOakStairs());
		    this.Register("minecraft:spruce_stairs", () => new SpruceStairs());
		    this.Register("minecraft:birch_stairs", () => new BirchStairs());
		    this.Register("minecraft:jungle_stairs", () => new JungleStairs());
		    this.Register("minecraft:oak_stairs", () => new OakStairs());
		    
		    this.Register("minecraft:water", () => new Water());
		    this.Register("minecraft:lava", () => new Lava());
		    this.Register("minecraft:kelp", () => new Kelp());
		    this.Register("minecraft:seagrass", () => new SeaGrass());
		    
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
		    this.Register("minecraft:white_stained_glass", () => new Glass());
		    this.Register("minecraft:orange_stained_glass", () => new Glass());
		    this.Register("minecraft:magenta_stained_glass", () => new Glass());
		    this.Register("minecraft:light_blue_stained_glass", () => new Glass());
		    this.Register("minecraft:yellow_stained_glass", () => new Glass());
		    this.Register("minecraft:lime_stained_glass", () => new Glass());
		    this.Register("minecraft:pink_stained_glass", () => new Glass());
		    this.Register("minecraft:gray_stained_glass", () => new Glass());
		    this.Register("minecraft:light_gray_stained_glass", () => new Glass());
		    this.Register("minecraft:purple_stained_glass", () => new Glass());
		    this.Register("minecraft:blue_stained_glass", () => new Glass());
		    this.Register("minecraft:brown_stained_glass", () => new Glass());
		    this.Register("minecraft:green_stained_glass", () => new Glass());
		    this.Register("minecraft:red_stained_glass", () => new Glass());
		    this.Register("minecraft:black_stained_glass", () => new Glass());
		    this.Register("minecraft:cyan_stained_glass", () => new Glass());
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
	    }
    }
}