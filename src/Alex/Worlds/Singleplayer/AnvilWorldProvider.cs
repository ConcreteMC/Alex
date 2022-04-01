using System.Collections.Generic;
using Alex.Blocks;

namespace Alex.Worlds.Singleplayer
{
	public class AnvilWorldProvider
	{
		private static uint GetBlockStateId(string name)
		{
			var res = BlockFactory.GetBlockState(name);

			if (res == null) return 0;

			return res.Id;
		}

		public static Dictionary<uint, uint> BlockStateMapper = new Dictionary<uint, uint>();

		public static void LoadBlockConverter()
		{
			if (BlockStateMapper.Count > 0) return;
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(0, 0), GetBlockStateId("minecraft:air"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(1, 0), GetBlockStateId("minecraft:stone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(1, 1), GetBlockStateId("minecraft:granite"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(1, 2), GetBlockStateId("minecraft:polished_granite"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(1, 3), GetBlockStateId("minecraft:diorite"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(1, 4), GetBlockStateId("minecraft:polished_diorite"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(1, 5), GetBlockStateId("minecraft:andesite"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(1, 6), GetBlockStateId("minecraft:polished_andesite"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(2, 0), GetBlockStateId("minecraft:grass_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(3, 0), GetBlockStateId("minecraft:dirt"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(3, 1), GetBlockStateId("minecraft:coarse_dirt"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(3, 2), GetBlockStateId("minecraft:podzol"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(4, 0), GetBlockStateId("minecraft:cobblestone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(5, 0), GetBlockStateId("minecraft:oak_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(5, 1), GetBlockStateId("minecraft:spruce_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(5, 2), GetBlockStateId("minecraft:birch_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(5, 3), GetBlockStateId("minecraft:jungle_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(5, 4), GetBlockStateId("minecraft:acacia_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(5, 5), GetBlockStateId("minecraft:dark_oak_planks"));

			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:oak_sapling"));

			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:spruce_sapling"));

			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:birch_sapling"));

			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:jungle_sapling"));

			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:acacia_sapling"));

			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:dark_oak_sapling"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(7, 0), GetBlockStateId("minecraft:bedrock"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(8, 0), GetBlockStateId("minecraft:flowing_water"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(9, 0), GetBlockStateId("minecraft:water"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(10, 0), GetBlockStateId("minecraft:flowing_lava"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(11, 0), GetBlockStateId("minecraft:lava"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(12, 0), GetBlockStateId("minecraft:sand"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(12, 1), GetBlockStateId("minecraft:red_sand"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(13, 0), GetBlockStateId("minecraft:gravel"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(14, 0), GetBlockStateId("minecraft:gold_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(15, 0), GetBlockStateId("minecraft:iron_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(16, 0), GetBlockStateId("minecraft:coal_ore"));

			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:oak_log"));

			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:spruce_log"));

			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:birch_log"));

			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:jungle_log"));

			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:oak_leaves"));

			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:spruce_leaves"));

			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:birch_leaves"));

			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 4)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:jungle_leaves"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(19, 0), GetBlockStateId("minecraft:sponge"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(19, 1), GetBlockStateId("minecraft:wet_sponge"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(20, 0), GetBlockStateId("minecraft:glass"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(21, 0), GetBlockStateId("minecraft:lapis_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(22, 0), GetBlockStateId("minecraft:lapis_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(23, 0), GetBlockStateId("minecraft:dispenser"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(24, 0), GetBlockStateId("minecraft:sandstone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(24, 1), GetBlockStateId("minecraft:chiseled_sandstone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(24, 2), GetBlockStateId("minecraft:cut_sandstone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(25, 0), GetBlockStateId("minecraft:note_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(26, 0), GetBlockStateId("minecraft:red_bed"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(27, 0), GetBlockStateId("minecraft:powered_rail"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(28, 0), GetBlockStateId("minecraft:detector_rail"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(29, 0), GetBlockStateId("minecraft:sticky_piston"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(30, 0), GetBlockStateId("minecraft:cobweb"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(31, 0), GetBlockStateId("minecraft:grass")); // dead_bush
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(31, 1), GetBlockStateId("minecraft:grass"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(31, 2), GetBlockStateId("minecraft:fern"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(32, 0), GetBlockStateId("minecraft:dead_bush"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(33, 0), GetBlockStateId("minecraft:piston"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(34, 0), GetBlockStateId("minecraft:piston_head"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 0), GetBlockStateId("minecraft:white_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 1), GetBlockStateId("minecraft:orange_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 2), GetBlockStateId("minecraft:magenta_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 3), GetBlockStateId("minecraft:light_blue_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 4), GetBlockStateId("minecraft:yellow_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 5), GetBlockStateId("minecraft:lime_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 6), GetBlockStateId("minecraft:pink_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 7), GetBlockStateId("minecraft:gray_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 8), GetBlockStateId("minecraft:light_gray_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 9), GetBlockStateId("minecraft:cyan_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 10), GetBlockStateId("minecraft:purple_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 11), GetBlockStateId("minecraft:blue_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 12), GetBlockStateId("minecraft:brown_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 13), GetBlockStateId("minecraft:green_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 14), GetBlockStateId("minecraft:red_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(35, 15), GetBlockStateId("minecraft:black_wool"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(37, 0), GetBlockStateId("minecraft:dandelion"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 0), GetBlockStateId("minecraft:poppy"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 1), GetBlockStateId("minecraft:blue_orchid"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 2), GetBlockStateId("minecraft:allium"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 3), GetBlockStateId("minecraft:azure_bluet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 4), GetBlockStateId("minecraft:red_tulip"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 5), GetBlockStateId("minecraft:orange_tulip"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 6), GetBlockStateId("minecraft:white_tulip"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 7), GetBlockStateId("minecraft:pink_tulip"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(38, 8), GetBlockStateId("minecraft:oxeye_daisy"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(39, 0), GetBlockStateId("minecraft:brown_mushroom"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(40, 0), GetBlockStateId("minecraft:red_mushroom"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(41, 0), GetBlockStateId("minecraft:gold_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(42, 0), GetBlockStateId("minecraft:iron_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 0), GetBlockStateId("minecraft:oak_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 1), GetBlockStateId("minecraft:sandstone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 2), GetBlockStateId("minecraft:oak_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 3), GetBlockStateId("minecraft:cobblestone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 4), GetBlockStateId("minecraft:bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 5), GetBlockStateId("minecraft:stone_bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 6), GetBlockStateId("minecraft:quartz_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 7), GetBlockStateId("minecraft:nether_bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 8), GetBlockStateId("minecraft:smooth_stone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 9), GetBlockStateId("minecraft:smooth_sandstone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(43, 14), GetBlockStateId("minecraft:smooth_quartz"));

			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:stone_slab"));

			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:sandstone_slab"));

			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:petrified_oak_slab"));

			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:cobblestone_slab"));

			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:brick_slab"));

			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:stone_brick_slab"));

			for (byte Índice_Data = 6; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:quartz_slab"));

			for (byte Índice_Data = 7; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:nether_brick_slab"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(45, 0), GetBlockStateId("minecraft:bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(46, 0), GetBlockStateId("minecraft:tnt"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(47, 0), GetBlockStateId("minecraft:bookshelf"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(48, 0), GetBlockStateId("minecraft:mossy_cobblestone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(49, 0), GetBlockStateId("minecraft:obsidian"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(50, 0), GetBlockStateId("minecraft:torch"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(51, 0), GetBlockStateId("minecraft:fire"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(52, 0), GetBlockStateId("minecraft:mob_spawner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(53, 0), GetBlockStateId("minecraft:oak_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(54, 0), GetBlockStateId("minecraft:chest"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(55, 0), GetBlockStateId("minecraft:redstone_wire"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(56, 0), GetBlockStateId("minecraft:diamond_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(57, 0), GetBlockStateId("minecraft:diamond_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(58, 0), GetBlockStateId("minecraft:crafting_table"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(59, 0), GetBlockStateId("minecraft:wheat"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(60, 0), GetBlockStateId("minecraft:farmland"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(61, 0), GetBlockStateId("minecraft:furnace"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(62, 0), GetBlockStateId("minecraft:furnace"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(63, 0), GetBlockStateId("minecraft:sign"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(64, 0), GetBlockStateId("minecraft:oak_door"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(65, 0), GetBlockStateId("minecraft:ladder"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(66, 0), GetBlockStateId("minecraft:rail"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(67, 0), GetBlockStateId("minecraft:cobblestone_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(68, 0), GetBlockStateId("minecraft:sign"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(69, 0), GetBlockStateId("minecraft:lever"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(70, 0), GetBlockStateId("minecraft:stone_pressure_plate"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(71, 0), GetBlockStateId("minecraft:iron_door"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(72, 0), GetBlockStateId("minecraft:oak_pressure_plate"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(73, 0), GetBlockStateId("minecraft:redstone_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(74, 0), GetBlockStateId("minecraft:redstone_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(75, 0), GetBlockStateId("minecraft:redstone_torch"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(76, 0), GetBlockStateId("minecraft:redstone_torch"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(77, 0), GetBlockStateId("minecraft:stone_button"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(78, 0), GetBlockStateId("minecraft:snow"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(79, 0), GetBlockStateId("minecraft:ice"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(80, 0), GetBlockStateId("minecraft:snow_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(81, 0), GetBlockStateId("minecraft:cactus"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(82, 0), GetBlockStateId("minecraft:clay"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(83, 0), GetBlockStateId("minecraft:sugar_cane"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(84, 0), GetBlockStateId("minecraft:jukebox"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(85, 0), GetBlockStateId("minecraft:oak_fence"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(86, 0), GetBlockStateId("minecraft:carved_pumpkin"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(87, 0), GetBlockStateId("minecraft:netherrack"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(88, 0), GetBlockStateId("minecraft:soul_sand"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(89, 0), GetBlockStateId("minecraft:glowstone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(90, 0), GetBlockStateId("minecraft:portal"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(91, 0), GetBlockStateId("minecraft:jack_o_lantern"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(92, 0), GetBlockStateId("minecraft:cake"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(93, 0), GetBlockStateId("minecraft:repeater"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(94, 0), GetBlockStateId("minecraft:repeater"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(95, 0), GetBlockStateId("minecraft:white_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 1), GetBlockStateId("minecraft:orange_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 2), GetBlockStateId("minecraft:magenta_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 3), GetBlockStateId("minecraft:light_blue_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 4), GetBlockStateId("minecraft:yellow_stained_glass"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(95, 5), GetBlockStateId("minecraft:lime_stained_glass"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(95, 6), GetBlockStateId("minecraft:pink_stained_glass"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(95, 7), GetBlockStateId("minecraft:gray_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 8), GetBlockStateId("minecraft:light_gray_stained_glass"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(95, 9), GetBlockStateId("minecraft:cyan_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 10), GetBlockStateId("minecraft:purple_stained_glass"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(95, 11), GetBlockStateId("minecraft:blue_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 12), GetBlockStateId("minecraft:brown_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 13), GetBlockStateId("minecraft:green_stained_glass"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(95, 14), GetBlockStateId("minecraft:red_stained_glass"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(95, 15), GetBlockStateId("minecraft:black_stained_glass"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(96, 0), GetBlockStateId("minecraft:oak_trapdoor"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(97, 0), GetBlockStateId("minecraft:infested_stone"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(97, 1), GetBlockStateId("minecraft:infested_cobblestone"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(97, 2), GetBlockStateId("minecraft:infested_stone_bricks"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(97, 3), GetBlockStateId("minecraft:infested_mossy_stone_bricks"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(97, 4), GetBlockStateId("minecraft:infested_cracked_stone_bricks"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(97, 5), GetBlockStateId("minecraft:infested_chiseled_stone_bricks"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(98, 0), GetBlockStateId("minecraft:stone_bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(98, 1), GetBlockStateId("minecraft:mossy_stone_bricks"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(98, 3), GetBlockStateId("minecraft:chiseled_stone_bricks"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(98, 4), GetBlockStateId("minecraft:cracked_stone_bricks"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(99, 0), GetBlockStateId("minecraft:brown_mushroom_block"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(100, 0), GetBlockStateId("minecraft:red_mushroom_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(101, 0), GetBlockStateId("minecraft:iron_bars"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(102, 0), GetBlockStateId("minecraft:glass_pane"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(103, 0), GetBlockStateId("minecraft:melon_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(104, 0), GetBlockStateId("minecraft:pumpkin_stem"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(105, 0), GetBlockStateId("minecraft:melon_stem"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(106, 0), GetBlockStateId("minecraft:vine"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(107, 0), GetBlockStateId("minecraft:oak_fence_gate"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(108, 0), GetBlockStateId("minecraft:brick_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(109, 0), GetBlockStateId("minecraft:stone_brick_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(110, 0), GetBlockStateId("minecraft:mycelium"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(111, 0), GetBlockStateId("minecraft:lily_pad"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(112, 0), GetBlockStateId("minecraft:nether_bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(113, 0), GetBlockStateId("minecraft:nether_brick_fence"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(114, 0), GetBlockStateId("minecraft:nether_brick_stairs"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(115, 0), GetBlockStateId("minecraft:nether_wart"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(116, 0), GetBlockStateId("minecraft:enchanting_table"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(117, 0), GetBlockStateId("minecraft:brewing_stand"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(118, 0), GetBlockStateId("minecraft:cauldron"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(119, 0), GetBlockStateId("minecraft:end_portal"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(120, 0), GetBlockStateId("minecraft:end_portal_frame"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(121, 0), GetBlockStateId("minecraft:end_stone"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(122, 0), GetBlockStateId("minecraft:dragon_egg"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(123, 0), GetBlockStateId("minecraft:redstone_lamp"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(124, 0), GetBlockStateId("minecraft:redstone_lamp"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(125, 0), GetBlockStateId("minecraft:oak_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(125, 1), GetBlockStateId("minecraft:spruce_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(125, 2), GetBlockStateId("minecraft:birch_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(125, 3), GetBlockStateId("minecraft:jungle_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(125, 4), GetBlockStateId("minecraft:acacia_planks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(125, 5), GetBlockStateId("minecraft:dark_oak_planks"));

			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:oak_slab"));

			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:spruce_slab"));

			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:birch_slab"));

			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:jungle_slab"));

			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:acacia_slab"));

			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:dark_oak_slab"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(127, 0), GetBlockStateId("minecraft:cocoa"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(128, 0), GetBlockStateId("minecraft:sandstone_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(129, 0), GetBlockStateId("minecraft:emerald_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(130, 0), GetBlockStateId("minecraft:ender_chest"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(131, 0), GetBlockStateId("minecraft:tripwire_hook"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(132, 0), GetBlockStateId("minecraft:tripwire"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(133, 0), GetBlockStateId("minecraft:emerald_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(134, 0), GetBlockStateId("minecraft:spruce_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(135, 0), GetBlockStateId("minecraft:birch_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(136, 0), GetBlockStateId("minecraft:jungle_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(137, 0), GetBlockStateId("minecraft:command_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(138, 0), GetBlockStateId("minecraft:beacon"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(139, 0), GetBlockStateId("minecraft:cobblestone_wall"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(139, 1), GetBlockStateId("minecraft:mossy_cobblestone_wall"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(140, 0), GetBlockStateId("minecraft:flower_pot"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(141, 0), GetBlockStateId("minecraft:carrots"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(142, 0), GetBlockStateId("minecraft:potatoes"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(143, 0), GetBlockStateId("minecraft:oak_button"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(144, 0), GetBlockStateId("minecraft:skeleton_skull"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(144, 1), GetBlockStateId("minecraft:wither_skeleton_skull"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(144, 2), GetBlockStateId("minecraft:zombie_head"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(144, 3), GetBlockStateId("minecraft:player_head"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(144, 4), GetBlockStateId("minecraft:creeper_head"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(144, 5), GetBlockStateId("minecraft:dragon_head"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(145, 0), GetBlockStateId("minecraft:anvil"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(145, 1), GetBlockStateId("minecraft:chipped_anvil"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(145, 2), GetBlockStateId("minecraft:damaged_anvil"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(146, 0), GetBlockStateId("minecraft:trapped_chest"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(147, 0), GetBlockStateId("minecraft:light_weighted_pressure_plate"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(148, 0), GetBlockStateId("minecraft:heavy_weighted_pressure_plate"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(149, 0), GetBlockStateId("minecraft:comparator"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(150, 0), GetBlockStateId("minecraft:comparator"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(151, 0), GetBlockStateId("minecraft:daylight_detector"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(152, 0), GetBlockStateId("minecraft:redstone_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(153, 0), GetBlockStateId("minecraft:nether_quartz_ore"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(154, 0), GetBlockStateId("minecraft:hopper"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(155, 0), GetBlockStateId("minecraft:quartz_block"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(155, 1), GetBlockStateId("minecraft:chiseled_quartz_block"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(155, 2), GetBlockStateId("minecraft:quartz_pillar"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(156, 0), GetBlockStateId("minecraft:quartz_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(157, 0), GetBlockStateId("minecraft:activator_rail"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(158, 0), GetBlockStateId("minecraft:dropper"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 0), GetBlockStateId("minecraft:white_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 1), GetBlockStateId("minecraft:orange_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 2), GetBlockStateId("minecraft:magenta_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(159, 3), GetBlockStateId("minecraft:light_blue_terracotta"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 4), GetBlockStateId("minecraft:yellow_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 5), GetBlockStateId("minecraft:lime_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 6), GetBlockStateId("minecraft:pink_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 7), GetBlockStateId("minecraft:gray_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(159, 8), GetBlockStateId("minecraft:light_gray_terracotta"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 9), GetBlockStateId("minecraft:cyan_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 10), GetBlockStateId("minecraft:purple_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 11), GetBlockStateId("minecraft:blue_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 12), GetBlockStateId("minecraft:brown_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 13), GetBlockStateId("minecraft:green_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 14), GetBlockStateId("minecraft:red_terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(159, 15), GetBlockStateId("minecraft:black_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 0), GetBlockStateId("minecraft:white_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 1), GetBlockStateId("minecraft:orange_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 2), GetBlockStateId("minecraft:magenta_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 3), GetBlockStateId("minecraft:light_blue_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 4), GetBlockStateId("minecraft:yellow_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 5), GetBlockStateId("minecraft:lime_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 6), GetBlockStateId("minecraft:pink_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 7), GetBlockStateId("minecraft:gray_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 8), GetBlockStateId("minecraft:light_gray_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 9), GetBlockStateId("minecraft:cyan_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 10), GetBlockStateId("minecraft:purple_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 11), GetBlockStateId("minecraft:blue_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 12), GetBlockStateId("minecraft:brown_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 13), GetBlockStateId("minecraft:green_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 14), GetBlockStateId("minecraft:red_stained_glass_pane"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(160, 15), GetBlockStateId("minecraft:black_stained_glass_pane"));

			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 2)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(161, Índice_Data), GetBlockStateId("minecraft:acacia_leaves"));

			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 2)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(161, Índice_Data), GetBlockStateId("minecraft:dark_oak_leaves"));

			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 2)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(162, Índice_Data), GetBlockStateId("minecraft:acacia_log"));

			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 2)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(162, Índice_Data), GetBlockStateId("minecraft:dark_oak_log"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(163, 0), GetBlockStateId("minecraft:acacia_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(164, 0), GetBlockStateId("minecraft:dark_oak_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(165, 0), GetBlockStateId("minecraft:slime_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(166, 0), GetBlockStateId("minecraft:barrier"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(167, 0), GetBlockStateId("minecraft:iron_trapdoor"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(168, 0), GetBlockStateId("minecraft:prismarine"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(168, 1), GetBlockStateId("minecraft:dark_prismarine"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(168, 2), GetBlockStateId("minecraft:prismarine_bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(169, 0), GetBlockStateId("minecraft:sea_lantern"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(170, 0), GetBlockStateId("minecraft:hay_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 0), GetBlockStateId("minecraft:white_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 1), GetBlockStateId("minecraft:orange_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 2), GetBlockStateId("minecraft:magenta_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 3), GetBlockStateId("minecraft:light_blue_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 4), GetBlockStateId("minecraft:yellow_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 5), GetBlockStateId("minecraft:lime_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 6), GetBlockStateId("minecraft:pink_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 7), GetBlockStateId("minecraft:gray_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 8), GetBlockStateId("minecraft:light_gray_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 9), GetBlockStateId("minecraft:cyan_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 10), GetBlockStateId("minecraft:purple_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 11), GetBlockStateId("minecraft:blue_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 12), GetBlockStateId("minecraft:brown_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 13), GetBlockStateId("minecraft:green_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 14), GetBlockStateId("minecraft:red_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(171, 15), GetBlockStateId("minecraft:black_carpet"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(172, 0), GetBlockStateId("minecraft:terracotta"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(173, 0), GetBlockStateId("minecraft:coal_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(174, 0), GetBlockStateId("minecraft:packed_ice"));

			//for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8) BlockStateMapper.Add(BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:sunflower"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:lilac"));

			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:tall_grass"));

			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:large_fern"));

			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:rose_bush"));

			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8)
				BlockStateMapper.Add(
					BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:peony"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 0), GetBlockStateId("minecraft:white_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 1), GetBlockStateId("minecraft:orange_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 2), GetBlockStateId("minecraft:magenta_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 3), GetBlockStateId("minecraft:light_blue_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 4), GetBlockStateId("minecraft:yellow_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 5), GetBlockStateId("minecraft:lime_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 6), GetBlockStateId("minecraft:pink_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 7), GetBlockStateId("minecraft:gray_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 8), GetBlockStateId("minecraft:light_gray_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 9), GetBlockStateId("minecraft:cyan_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 10), GetBlockStateId("minecraft:purple_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 11), GetBlockStateId("minecraft:blue_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 12), GetBlockStateId("minecraft:brown_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 13), GetBlockStateId("minecraft:green_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 14), GetBlockStateId("minecraft:red_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(176, 15), GetBlockStateId("minecraft:black_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 0), GetBlockStateId("minecraft:white_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 1), GetBlockStateId("minecraft:orange_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 2), GetBlockStateId("minecraft:magenta_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 3), GetBlockStateId("minecraft:light_blue_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 4), GetBlockStateId("minecraft:yellow_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 5), GetBlockStateId("minecraft:lime_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 6), GetBlockStateId("minecraft:pink_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 7), GetBlockStateId("minecraft:gray_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 8), GetBlockStateId("minecraft:light_gray_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 9), GetBlockStateId("minecraft:cyan_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 10), GetBlockStateId("minecraft:purple_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 11), GetBlockStateId("minecraft:blue_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 12), GetBlockStateId("minecraft:brown_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 13), GetBlockStateId("minecraft:green_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 14), GetBlockStateId("minecraft:red_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(177, 15), GetBlockStateId("minecraft:black_banner"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(178, 0), GetBlockStateId("minecraft:daylight_detector"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(179, 0), GetBlockStateId("minecraft:red_sandstone"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(179, 1), GetBlockStateId("minecraft:chiseled_red_sandstone"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(179, 2), GetBlockStateId("minecraft:cut_red_sandstone"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(180, 0), GetBlockStateId("minecraft:red_sandstone_stairs"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(181, 0), GetBlockStateId("minecraft:red_sandstone"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(181, 8), GetBlockStateId("minecraft:smooth_red_sandstone"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(182, 0), GetBlockStateId("minecraft:red_sandstone_slab"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(183, 0), GetBlockStateId("minecraft:spruce_fence_gate"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(184, 0), GetBlockStateId("minecraft:birch_fence_gate"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(185, 0), GetBlockStateId("minecraft:jungle_fence_gate"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(186, 0), GetBlockStateId("minecraft:dark_oak_fence_gate"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(187, 0), GetBlockStateId("minecraft:acacia_fence_gate"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(188, 0), GetBlockStateId("minecraft:spruce_fence"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(189, 0), GetBlockStateId("minecraft:birch_fence"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(190, 0), GetBlockStateId("minecraft:jungle_fence"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(191, 0), GetBlockStateId("minecraft:acacia_fence"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(192, 0), GetBlockStateId("minecraft:dark_oak_fence"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(193, 0), GetBlockStateId("minecraft:spruce_door"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(194, 0), GetBlockStateId("minecraft:birch_door"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(195, 0), GetBlockStateId("minecraft:jungle_door"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(196, 0), GetBlockStateId("minecraft:acacia_door"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(197, 0), GetBlockStateId("minecraft:dark_oak_door"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(198, 0), GetBlockStateId("minecraft:end_rod"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(199, 0), GetBlockStateId("minecraft:chorus_plant"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(200, 0), GetBlockStateId("minecraft:chorus_flower"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(201, 0), GetBlockStateId("minecraft:purpur_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(202, 0), GetBlockStateId("minecraft:purpur_pillar"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(203, 0), GetBlockStateId("minecraft:purpur_stairs"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(204, 0), GetBlockStateId("minecraft:purpur_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(205, 0), GetBlockStateId("minecraft:purpur_slab"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(206, 0), GetBlockStateId("minecraft:end_stone_bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(207, 0), GetBlockStateId("minecraft:beetroots"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(208, 0), GetBlockStateId("minecraft:grass_path"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(209, 0), GetBlockStateId("minecraft:end_gateway"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(210, 0), GetBlockStateId("minecraft:repeating_command_block"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(211, 0), GetBlockStateId("minecraft:chain_command_block"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(212, 0), GetBlockStateId("minecraft:frosted_ice"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(213, 0), GetBlockStateId("minecraft:magma_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(214, 0), GetBlockStateId("minecraft:nether_wart_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(215, 0), GetBlockStateId("minecraft:red_nether_bricks"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(216, 0), GetBlockStateId("minecraft:bone_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(217, 0), GetBlockStateId("minecraft:structure_void"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(218, 0), GetBlockStateId("minecraft:observer"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(219, 0), GetBlockStateId("minecraft:white_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(220, 0), GetBlockStateId("minecraft:orange_shulker_box"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(221, 0), GetBlockStateId("minecraft:magenta_shulker_box"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(222, 0), GetBlockStateId("minecraft:light_blue_shulker_box"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(223, 0), GetBlockStateId("minecraft:yellow_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(224, 0), GetBlockStateId("minecraft:lime_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(225, 0), GetBlockStateId("minecraft:pink_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(226, 0), GetBlockStateId("minecraft:gray_shulker_box"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(227, 0), GetBlockStateId("minecraft:light_gray_shulker_box"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(228, 0), GetBlockStateId("minecraft:cyan_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(229, 0), GetBlockStateId("minecraft:purple_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(230, 0), GetBlockStateId("minecraft:blue_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(231, 0), GetBlockStateId("minecraft:brown_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(232, 0), GetBlockStateId("minecraft:green_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(233, 0), GetBlockStateId("minecraft:red_shulker_box"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(234, 0), GetBlockStateId("minecraft:black_shulker_box"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(235, 0), GetBlockStateId("minecraft:white_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(236, 0), GetBlockStateId("minecraft:orange_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(237, 0), GetBlockStateId("minecraft:magenta_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(238, 0), GetBlockStateId("minecraft:light_blue_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(239, 0), GetBlockStateId("minecraft:yellow_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(240, 0), GetBlockStateId("minecraft:lime_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(241, 0), GetBlockStateId("minecraft:pink_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(242, 0), GetBlockStateId("minecraft:gray_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(243, 0), GetBlockStateId("minecraft:light_gray_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(244, 0), GetBlockStateId("minecraft:cyan_glazed_terra cotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(245, 0), GetBlockStateId("minecraft:purple_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(246, 0), GetBlockStateId("minecraft:blue_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(247, 0), GetBlockStateId("minecraft:brown_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(248, 0), GetBlockStateId("minecraft:green_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(249, 0), GetBlockStateId("minecraft:red_glazed_terracotta"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(250, 0), GetBlockStateId("minecraft:black_glazed_terracotta"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 0), GetBlockStateId("minecraft:white_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 1), GetBlockStateId("minecraft:orange_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 2), GetBlockStateId("minecraft:magenta_concrete"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(251, 3), GetBlockStateId("minecraft:light_blue_concrete"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 4), GetBlockStateId("minecraft:yellow_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 5), GetBlockStateId("minecraft:lime_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 6), GetBlockStateId("minecraft:pink_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 7), GetBlockStateId("minecraft:gray_concrete"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(251, 8), GetBlockStateId("minecraft:light_gray_concrete"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 9), GetBlockStateId("minecraft:cyan_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 10), GetBlockStateId("minecraft:purple_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 11), GetBlockStateId("minecraft:blue_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 12), GetBlockStateId("minecraft:brown_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 13), GetBlockStateId("minecraft:green_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 14), GetBlockStateId("minecraft:red_concrete"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(251, 15), GetBlockStateId("minecraft:black_concrete"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 0), GetBlockStateId("minecraft:white_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 1), GetBlockStateId("minecraft:orange_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 2), GetBlockStateId("minecraft:magenta_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 3), GetBlockStateId("minecraft:light_blue_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 4), GetBlockStateId("minecraft:yellow_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 5), GetBlockStateId("minecraft:lime_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 6), GetBlockStateId("minecraft:pink_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 7), GetBlockStateId("minecraft:gray_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 8), GetBlockStateId("minecraft:light_gray_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 9), GetBlockStateId("minecraft:cyan_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 10), GetBlockStateId("minecraft:purple_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 11), GetBlockStateId("minecraft:blue_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 12), GetBlockStateId("minecraft:brown_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 13), GetBlockStateId("minecraft:green_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 14), GetBlockStateId("minecraft:red_concrete_powder"));

			BlockStateMapper.Add(
				BlockFactory.GetBlockStateID(252, 15), GetBlockStateId("minecraft:black_concrete_powder"));

			BlockStateMapper.Add(BlockFactory.GetBlockStateID(255, 0), GetBlockStateId("minecraft:structure_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(255, 1), GetBlockStateId("minecraft:structure_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(255, 2), GetBlockStateId("minecraft:structure_block"));
			BlockStateMapper.Add(BlockFactory.GetBlockStateID(255, 3), GetBlockStateId("minecraft:structure_block"));

			// 36, 253, 254
			// Agregar el resto de Datas posibles para cada ID con el valor del primero:
			for (short Índice_ID = 0; Índice_ID < 256; Índice_ID++)
			{
				uint Índice_ID_Data_Cero = BlockFactory.GetBlockStateID(Índice_ID, 0);

				if (!BlockStateMapper.ContainsKey(Índice_ID_Data_Cero))
				{
					BlockStateMapper.Add(Índice_ID_Data_Cero, GetBlockStateId("minecraft:air"));
				}

				for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data++)
				{
					uint Índice_ID_Data = BlockFactory.GetBlockStateID(Índice_ID, Índice_Data);

					if (!BlockStateMapper.ContainsKey(Índice_ID_Data))
					{
						BlockStateMapper.Add(Índice_ID_Data, BlockStateMapper[Índice_ID_Data_Cero]);
						//_1p12to1p13.Add(Índice_ID_Data, _1p12to1p13[BlockFactory.GetBlockStateID(152, 0)));
					}
				}
				//else MessageBox.Show(Índice_ID.ToString(), "No dic");
			}
		}
	}
}