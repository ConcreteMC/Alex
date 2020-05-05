using Alex.API.Resources;
using Alex.Blocks.Minecraft;

namespace Alex.Blocks
{
    public class BlockRegistry : RegistryBase<Block>
    {
        public BlockRegistry() : base("block")
        {
			this.Register(() => new Air().WithLocation("minecraft:air"));
			this.Register(() => new Air().WithLocation("minecraft:cave_air"));

			this.Register(() => new Stone().WithLocation("minecraft:stone"));
			this.Register(() => new Dirt().WithLocation("minecraft:dirt"));
			this.Register(() => new Podzol().WithLocation("minecraft:podzol"));
			this.Register(() => new Cobblestone().WithLocation("minecraft:cobblestone"));
			this.Register(() => new Bedrock().WithLocation("minecraft:bedrock"));
			this.Register(() => new Sand().WithLocation("minecraft:sand"));
			this.Register(() => new Gravel().WithLocation("minecraft:gravel"));
			this.Register(() => new Sponge().WithLocation("minecraft:sponge"));
			this.Register(() => new Glass().WithLocation("minecraft:glass"));
			this.Register(() => new Dispenser().WithLocation("minecraft:dispenser"));
			this.Register(() => new Sandstone().WithLocation("minecraft:sandstone"));
			this.Register(() => new NoteBlock().WithLocation("minecraft:note_block"));
			this.Register(() => new DetectorRail().WithLocation("minecraft:detector_rail"));
			this.Register(() => new Grass().WithLocation("minecraft:grass"));
			this.Register(() => new Fern().WithLocation("minecraft:fern"));
			this.Register(() => new Fern().WithLocation("minecraft:large_fern"));
			this.Register(() => new BrownMushroom().WithLocation("minecraft:brown_mushroom"));
			this.Register(() => new RedMushroom().WithLocation("minecraft:red_mushroom"));
			this.Register(() => new DeadBush().WithLocation("minecraft:dead_bush"));

			this.Register(() => new Tnt().WithLocation("minecraft:tnt"));
			this.Register(() => new Bookshelf().WithLocation("minecraft:bookshelf"));
			this.Register(() => new MossyCobblestone().WithLocation("minecraft:mossy_cobblestone"));
			this.Register(() => new Obsidian().WithLocation("minecraft:obsidian"));
			this.Register(() => new Fire().WithLocation("minecraft:fire"));
			this.Register(() => new MobSpawner().WithLocation("minecraft:mob_spawner"));
			this.Register(() => new Chest().WithLocation("minecraft:chest"));

			this.Register(() => new CraftingTable().WithLocation("minecraft:crafting_table"));
			this.Register(() => new Wheat().WithLocation("minecraft:wheat"));
			this.Register(() => new Farmland().WithLocation("minecraft:farmland"));
			this.Register(() => new Furnace().WithLocation("minecraft:furnace"));
			this.Register(() => new Ladder().WithLocation("minecraft:ladder"));
			this.Register(() => new Rail().WithLocation("minecraft:rail"));
			this.Register(() => new WallSign().WithLocation("minecraft:wall_sign"));

			this.Register(() => new Snow().WithLocation("minecraft:snow"));
			this.Register(() => new Ice().WithLocation("minecraft:ice"));
			this.Register(() => new Cactus().WithLocation("minecraft:cactus"));
			this.Register(() => new Clay().WithLocation("minecraft:clay"));
			this.Register(() => new Pumpkin().WithLocation("minecraft:pumpkin"));
			this.Register(() => new Netherrack().WithLocation("minecraft:netherrack"));
			this.Register(() => new SoulSand().WithLocation("minecraft:soul_sand"));
			this.Register(() => new Glowstone().WithLocation("minecraft:glowstone"));
			this.Register(() => new Portal().WithLocation("minecraft:portal"));
			this.Register(() => new Portal().WithLocation("minecraft:nether_portal"));
			this.Register(() => new Cake().WithLocation("minecraft:cake"));
			this.Register(() => new BrownMushroomBlock().WithLocation("minecraft:brown_mushroom_block"));
			this.Register(() => new RedMushroomBlock().WithLocation("minecraft:red_mushroom_block"));
			this.Register(() => new IronBars().WithLocation("minecraft:iron_bars"));
			
			this.Register(() => new Vine().WithLocation("minecraft:vine"));
			this.Register(() => new Mycelium().WithLocation("minecraft:mycelium"));
			this.Register(() => new NetherWart().WithLocation("minecraft:nether_wart"));
			this.Register(() => new EnchantingTable().WithLocation("minecraft:enchanting_table"));
			this.Register(() => new BrewingStand().WithLocation("minecraft:brewing_stand"));
			this.Register(() => new Cauldron().WithLocation("minecraft:cauldron"));
			this.Register(() => new EndPortal().WithLocation("minecraft:end_portal"));
			this.Register(() => new EndPortalFrame().WithLocation("minecraft:end_portal_frame"));
			this.Register(() => new EndStone().WithLocation("minecraft:end_stone"));
			this.Register(() => new DragonEgg().WithLocation("minecraft:dragon_egg"));
			this.Register(() => new RedstoneLamp().WithLocation("minecraft:redstone_lamp"));
			this.Register(() => new Cocoa().WithLocation("minecraft:cocoa"));
			this.Register(() => new EnderChest().WithLocation("minecraft:ender_chest"));
			this.Register(() => new TripwireHook().WithLocation("minecraft:tripwire_hook"));
			this.Register(() => new Tripwire().WithLocation("minecraft:tripwire"));
			this.Register(() => new Beacon().WithLocation("minecraft:beacon"));
			this.Register(() => new CobblestoneWall().WithLocation("minecraft:cobblestone_wall"));
			this.Register(() => new FlowerPot().WithLocation("minecraft:flower_pot"));
			this.Register(() => new Carrots().WithLocation("minecraft:carrots"));
			this.Register(() => new Potatoes().WithLocation("minecraft:potatoes"));
			this.Register(() => new Anvil().WithLocation("minecraft:anvil"));
			this.Register(() => new TrappedChest().WithLocation("minecraft:trapped_chest"));
			
			this.Register(() => new QuartzBlock().WithLocation("minecraft:quartz_block"));
			this.Register(() => new ActivatorRail().WithLocation("minecraft:activator_rail"));
			this.Register(() => new Dropper().WithLocation("minecraft:dropper"));
			
			this.Register(() => new Prismarine().WithLocation("minecraft:prismarine"));
			this.Register(() => new SeaLantern().WithLocation("minecraft:sea_lantern"));
			this.Register(() => new HayBlock().WithLocation("minecraft:hay_block"));
			this.Register(() => new CoalBlock().WithLocation("minecraft:coal_block"));
			this.Register(() => new PackedIce().WithLocation("minecraft:packed_ice"));
			this.Register(() => new TallGrass().WithLocation("minecraft:tall_grass"));
			this.Register(() => new RedSandstone().WithLocation("minecraft:red_sandstone"));
			this.Register(() => new EndRod().WithLocation("minecraft:end_rod"));
			this.Register(() => new ChorusPlant().WithLocation("minecraft:chorus_plant"));
			this.Register(() => new ChorusFlower().WithLocation("minecraft:chorus_flower"));
			this.Register(() => new PurpurBlock().WithLocation("minecraft:purpur_block"));
			this.Register(() => new GrassPath().WithLocation("minecraft:grass_path"));
			this.Register(() => new EndGateway().WithLocation("minecraft:end_gateway"));
			this.Register(() => new FrostedIce().WithLocation("minecraft:frosted_ice"));
			this.Register(() => new Observer().WithLocation("minecraft:observer"));
			this.Register(() => new GrassBlock().WithLocation("minecraft:grass_block"));
			this.Register(() => new PoweredRail().WithLocation("minecraft:powered_rail"));
			this.Register(() => new Bricks().WithLocation("minecraft:bricks"));
			this.Register(() => new Cobweb().WithLocation("minecraft:cobweb"));
			this.Register(() => new Dandelion().WithLocation("minecraft:dandelion"));
			this.Register(() => new Poppy().WithLocation("minecraft:poppy"));
			this.Register(() => new SugarCane().WithLocation("minecraft:sugar_cane"));
			this.Register(() => new Beetroots().WithLocation("minecraft:beetroots"));
			this.Register(() => new NetherWartBlock().WithLocation("minecraft:nether_wart_block"));
			this.Register(() => new Jukebox().WithLocation("minecraft:jukebox"));
			this.Register(() => new StoneBricks().WithLocation("minecraft:stone_bricks"));
			this.Register(() => new LilyPad().WithLocation("minecraft:lily_pad"));
			this.Register(() => new CommandBlock().WithLocation("minecraft:command_block"));
			this.Register(() => new NetherQuartzOre().WithLocation("minecraft:nether_quartz_ore"));
			this.Register(() => new SlimeBlock().WithLocation("minecraft:slime_block"));
			this.Register(() => new PurpurPillar().WithLocation("minecraft:purpur_pillar"));
			this.Register(() => new EndStoneBricks().WithLocation("minecraft:end_stone_bricks"));
			this.Register(() => new RepeatingCommandBlock().WithLocation("minecraft:repeating_command_block"));
			this.Register(() => new ChainCommandBlock().WithLocation("minecraft:chain_command_block"));
			this.Register(() => new MagmaBlock().WithLocation("minecraft:magma_block"));
			this.Register(() => new BoneBlock().WithLocation("minecraft:bone_block"));
			this.Register(() => new StructureBlock().WithLocation("minecraft:structure_block"));

			//Redstone
			this.Register(() => new Lever().WithLocation("minecraft:lever"));
			this.Register(() => new RedstoneWire().WithLocation("minecraft:redstone_wire"));
			this.Register(() => new Piston().WithLocation("minecraft:piston"));
			this.Register(() => new PistonHead().WithLocation("minecraft:piston_head"));
			this.Register(() => new StickyPiston().WithLocation("minecraft:sticky_piston"));
			this.Register(() => new DaylightDetector().WithLocation("minecraft:daylight_detector"));
			this.Register(() => new RedstoneBlock().WithLocation("minecraft:redstone_block"));
			this.Register(() => new Hopper().WithLocation("minecraft:hopper"));
			
			//Pressure plates
			this.Register(() => new LightWeightedPressurePlate().WithLocation("minecraft:light_weighted_pressure_plate"));
			this.Register(() => new HeavyWeightedPressurePlate().WithLocation("minecraft:heavy_weighted_pressure_plate"));
			this.Register(() => new StonePressurePlate().WithLocation("minecraft:stone_pressure_plate"));
			this.Register(() => new StonePressurePlate().WithLocation("minecraft:oak_pressure_plate"));
			
			//Torches
			this.Register(() => new Torch().WithLocation("minecraft:torch"));
			this.Register(() => new Torch(true).WithLocation("minecraft:wall_torch"));
			this.Register(() => new RedstoneTorch().WithLocation("minecraft:redstone_torch"));
			this.Register(() => new RedstoneTorch(true).WithLocation("minecraft:redstone_wall_torch"));
			
			//Buttons
			this.Register(() => new StoneButton().WithLocation("minecraft:stone_button"));
			this.Register(() => new OakButton().WithLocation("minecraft:oak_button"));
			this.Register(() => new SpruceButton().WithLocation("minecraft:spruce_button"));
			this.Register(() => new BirchButton().WithLocation("minecraft:birch_button"));
			this.Register(() => new JungleButton().WithLocation("minecraft:jungle_button"));
			this.Register(() => new AcaciaButton().WithLocation("minecraft:acacia_button"));
			this.Register(() => new DarkOakButton().WithLocation("minecraft:dark_oak_button"));

			//Terracotta
			this.Register(() => new WhiteGlazedTerracotta().WithLocation("minecraft:white_glazed_terracotta"));
			this.Register(() => new OrangeGlazedTerracotta().WithLocation("minecraft:orange_glazed_terracotta"));
			this.Register(() => new MagentaGlazedTerracotta().WithLocation("minecraft:magenta_glazed_terracotta"));
			this.Register(() => new LightBlueGlazedTerracotta().WithLocation("minecraft:light_blue_glazed_terracotta"));
			this.Register(() => new YellowGlazedTerracotta().WithLocation("minecraft:yellow_glazed_terracotta"));
			this.Register(() => new LimeGlazedTerracotta().WithLocation("minecraft:lime_glazed_terracotta"));
			this.Register(() => new PinkGlazedTerracotta().WithLocation("minecraft:pink_glazed_terracotta"));
			this.Register(() => new GrayGlazedTerracotta().WithLocation("minecraft:gray_glazed_terracotta"));
			this.Register(() => new CyanGlazedTerracotta().WithLocation("minecraft:cyan_glazed_terracotta"));
			this.Register(() => new PurpleGlazedTerracotta().WithLocation("minecraft:purple_glazed_terracotta"));
			this.Register(() => new BlueGlazedTerracotta().WithLocation("minecraft:blue_glazed_terracotta"));
			this.Register(() => new BrownGlazedTerracotta().WithLocation("minecraft:brown_glazed_terracotta"));
			this.Register(() => new GreenGlazedTerracotta().WithLocation("minecraft:green_glazed_terracotta"));
			this.Register(() => new RedGlazedTerracotta().WithLocation("minecraft:red_glazed_terracotta"));
			this.Register(() => new BlackGlazedTerracotta().WithLocation("minecraft:black_glazed_terracotta"));
			this.Register(() => new LightGrayGlazedTerracotta().WithLocation("minecraft:light_gray_glazed_terracotta"));
			this.Register(() => new Terracotta().WithLocation("minecraft:white_terracotta"));

			//Doors
			this.Register(() => new OakDoor().WithLocation("minecraft:oak_door"));
			this.Register(() => new SpruceDoor().WithLocation("minecraft:spruce_door"));
			this.Register(() => new BirchDoor().WithLocation("minecraft:birch_door"));
			this.Register(() => new JungleDoor().WithLocation("minecraft:jungle_door"));
			this.Register(() => new AcaciaDoor().WithLocation("minecraft:acacia_door"));
			this.Register(() => new DarkOakDoor().WithLocation("minecraft:dark_oak_door"));
			this.Register(() => new IronDoor().WithLocation("minecraft:iron_door"));

			//Trap doors
			this.Register(() => new Trapdoor().WithLocation("minecraft:iron_trapdoor"));
			this.Register(() => new Trapdoor().WithLocation("minecraft:spruce_trapdoor"));
			this.Register(() => new Trapdoor().WithLocation("minecraft:oak_trapdoor"));
			
			//Wooden Slabs
			this.Register(() => new OakSlab().WithLocation("minecraft:oak_slab"));
			this.Register(() => new SpruceSlab().WithLocation("minecraft:spruce_slab"));
			this.Register(() => new BirchSlab().WithLocation("minecraft:birch_slab"));
			this.Register(() => new JungleSlab().WithLocation("minecraft:jungle_slab"));
			this.Register(() => new AcaciaSlab().WithLocation("minecraft:acacia_slab"));
			this.Register(() => new DarkOakSlab().WithLocation("minecraft:dark_oak_slab"));
			
			//Stone slabs
			this.Register(() => new StoneSlab().WithLocation("minecraft:stone_slab"));
			this.Register(() => new PrismarineSlab().WithLocation("minecraft:prismarine_slab"));
			this.Register(() => new PrismarineBricksSlab().WithLocation("minecraft:prismarine_bricks_slab"));
			this.Register(() => new DarkPrismarineSlab().WithLocation("minecraft:dark_prismarine_slab"));
			this.Register(() => new SandstoneSlab().WithLocation("minecraft:sandstone_slab"));
			this.Register(() => new PetrifiedOakSlab().WithLocation("minecraft:petrified_oak_slab"));
			this.Register(() => new CobblestoneSlab().WithLocation("minecraft:cobblestone_slab"));
			this.Register(() => new BrickSlab().WithLocation("minecraft:brick_slab"));
			this.Register(() => new StoneBrickSlab().WithLocation("minecraft:stone_brick_slab"));
			this.Register(() => new NetherBrickSlab().WithLocation("minecraft:nether_brick_slab"));
			this.Register(() => new QuartzSlab().WithLocation("minecraft:quartz_slab"));
			this.Register(() => new RedSandstoneSlab().WithLocation("minecraft:red_sandstone_slab"));
			this.Register(() => new PurpurSlab().WithLocation("minecraft:purpur_slab"));

			//Leaves
			this.Register(() => new OakLeaves().WithLocation("minecraft:oak_leaves"));
			this.Register(() => new SpruceLeaves().WithLocation("minecraft:spruce_leaves"));
			this.Register(() => new BirchLeaves().WithLocation("minecraft:birch_leaves"));
			this.Register(() => new JungleLeaves().WithLocation("minecraft:jungle_leaves"));
			this.Register(() => new AcaciaLeaves().WithLocation("minecraft:acacia_leaves"));
			this.Register(() => new DarkOakLeaves().WithLocation("minecraft:dark_oak_leaves"));
			
			//Logs
			this.Register(() => new Log().WithLocation("minecraft:oak_log"));
			this.Register(() => new Log().WithLocation("minecraft:acacia_log"));
			this.Register(() => new Log().WithLocation("minecraft:jungle_log"));
			this.Register(() => new Log().WithLocation("minecraft:spruce_log"));
			this.Register(() => new Log().WithLocation("minecraft:birch_log"));
			
			//Planks
			this.Register(() => new Planks().WithLocation("minecraft:oak_planks"));
			this.Register(() => new Planks().WithLocation("minecraft:spruce_planks"));
			this.Register(() => new DarkOakPlanks().WithLocation("minecraft:dark_oak_planks"));

			//Fencing
			this.Register(() => new NetherBrickFence().WithLocation("minecraft:nether_brick_fence"));
			this.Register(() => new OakFence().WithLocation("minecraft:oak_fence"));
			this.Register(() => new FenceGate().WithLocation("minecraft:oak_fence_gate"));
			this.Register(() => new DarkOakFenceGate().WithLocation("minecraft:dark_oak_fence_gate"));
			this.Register(() => new Fence().WithLocation("minecraft:dark_oak_fence"));
			this.Register(() => new SpruceFenceGate().WithLocation("minecraft:spruce_fence_gate"));
			this.Register(() => new Fence().WithLocation("minecraft:spruce_fence"));
			this.Register(() => new BirchFenceGate().WithLocation("minecraft:birch_fence_gate"));
			this.Register(() => new BirchFence().WithLocation("minecraft:birch_fence"));
			this.Register(() => new JungleFenceGate().WithLocation("minecraft:jungle_fence_gate"));
			this.Register(() => new Fence().WithLocation("minecraft:jungle_fence"));
			this.Register(() => new AcaciaFenceGate().WithLocation("minecraft:acacia_fence_gate"));
			this.Register(() => new Fence().WithLocation("minecraft:acacia_fence"));

			//Stairs
			this.Register(() => new StoneStairs().WithLocation("minecraft:stone_stairs"));
			this.Register(() => new PurpurStairs().WithLocation("minecraft:purpur_stairs"));
			this.Register(() => new CobblestoneStairs().WithLocation("minecraft:cobblestone_stairs"));
			this.Register(() => new QuartzStairs().WithLocation("minecraft:quartz_stairs"));
			this.Register(() => new RedSandstoneStairs().WithLocation("minecraft:red_sandstone_stairs"));
			this.Register(() => new SandstoneStairs().WithLocation("minecraft:sandstone_stairs"));
			this.Register(() => new BrickStairs().WithLocation("minecraft:brick_stairs"));
			this.Register(() => new StoneBrickStairs().WithLocation("minecraft:stone_brick_stairs"));
			this.Register(() => new NetherBrickStairs().WithLocation("minecraft:nether_brick_stairs"));
			
			//Wooden stairs
			this.Register(() => new AcaciaStairs().WithLocation("minecraft:acacia_stairs"));
			this.Register(() => new DarkOakStairs().WithLocation("minecraft:dark_oak_stairs"));
			this.Register(() => new SpruceStairs().WithLocation("minecraft:spruce_stairs"));
			this.Register(() => new BirchStairs().WithLocation("minecraft:birch_stairs"));
			this.Register(() => new JungleStairs().WithLocation("minecraft:jungle_stairs"));
			this.Register(() => new OakStairs().WithLocation("minecraft:oak_stairs"));

			//Liquid
			this.Register(() => new Water().WithLocation("minecraft:water"));
			this.Register(() => new Lava().WithLocation("minecraft:lava"));
			this.Register(() => new Kelp().WithLocation("minecraft:kelp"));
			this.Register(() => new SeaGrass().WithLocation("minecraft:seagrass"));

			//Ores
			this.Register(() => new RedstoneOre().WithLocation("minecraft:redstone_ore"));
			this.Register(() => new GoldOre().WithLocation("minecraft:gold_ore"));
			this.Register(() => new IronOre().WithLocation("minecraft:iron_ore"));
			this.Register(() => new CoalOre().WithLocation("minecraft:coal_ore"));
			this.Register(() => new DiamondOre().WithLocation("minecraft:diamond_ore"));
			this.Register(() => new EmeraldOre().WithLocation("minecraft:emerald_ore"));
			this.Register(() => new LapisOre().WithLocation("minecraft:lapis_ore"));

			this.Register(() => new GoldBlock().WithLocation("minecraft:gold_block"));
			this.Register(() => new IronBlock().WithLocation("minecraft:iron_block"));
			this.Register(() => new DiamondBlock().WithLocation("minecraft:diamond_block"));
			this.Register(() => new EmeraldBlock().WithLocation("minecraft:emerald_block"));
			this.Register(() => new LapisBlock().WithLocation("minecraft:lapis_block"));

			//Plants
			this.Register(() => new Lilac().WithLocation("minecraft:lilac"));
			this.Register(() => new RoseBush().WithLocation("minecraft:rose_bush"));
			this.Register(() => new AzureBluet().WithLocation("minecraft:azure_bluet"));
			this.Register(() => new CornFlower().WithLocation("minecraft:corn_flower"));
			this.Register(() => new CornFlower().WithLocation("minecraft:cornflower"));
			this.Register(() => new OxeyeDaisy().WithLocation("minecraft:oxeye_daisy"));
			this.Register(() => new Stem().WithLocation("minecraft:attached_melon_stem"));
			this.Register(() => new Stem().WithLocation("minecraft:melon_stem"));
			this.Register(() => new MelonBlock().WithLocation("minecraft:melon_block"));
			this.Register(() => new PumpkinStem().WithLocation("minecraft:pumpkin_stem"));
			this.Register(() => new Sunflower().WithLocation("minecraft:sunflower"));
			this.Register(() => new Tulip().WithLocation("minecraft:red_tulip"));
			this.Register(() => new Tulip().WithLocation("minecraft:pink_tulip"));
			this.Register(() => new Tulip().WithLocation("minecraft:white_tulip"));
			this.Register(() => new Tulip().WithLocation("minecraft:orange_tulip"));
			this.Register(() => new Allium().WithLocation("minecraft:allium"));
			this.Register(() => new Lilac().WithLocation("minecraft:lily_of_the_valley"));
			
			this.Register(() => new Peony().WithLocation("minecraft:peony"));
			
			this.Register(() => new InvisibleBedrock(false).WithLocation("minecraft:barrier"));
            
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
			
            //Stained glass
            this.Register(() => new Glass().WithLocation("minecraft:white_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:orange_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:magenta_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:light_blue_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:yellow_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:lime_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:pink_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:gray_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:light_gray_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:purple_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:blue_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:brown_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:green_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:red_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:black_stained_glass"));
            this.Register(() => new Glass().WithLocation("minecraft:cyan_stained_glass"));
            
            this.Register(() => new GlassPane().WithLocation("minecraft:glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:white_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:orange_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:magenta_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:light_blue_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:yellow_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:lime_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:pink_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:gray_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:light_gray_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:purple_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:blue_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:brown_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:green_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:red_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:black_stained_glass_pane"));
            this.Register(() => new GlassPane().WithLocation("minecraft:cyan_stained_glass_pane"));
            
            this.Register(() => new Grindstone().WithLocation("minecraft:grindstone"));
            this.Register(() => new Bell().WithLocation("minecraft:bell"));
        }
    }
}