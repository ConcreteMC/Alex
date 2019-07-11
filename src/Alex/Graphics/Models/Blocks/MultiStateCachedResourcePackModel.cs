using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.ResourcePackLib.Json.BlockStates;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Graphics.Models.Blocks
{
	public class MultiPartModels
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiPartModels));
		public static BlockStateModel[] GetModels(IBlockState blockState, BlockStateResource resource)
		{
			List<BlockStateModel> resultingModels = new List<BlockStateModel>(resource.Parts.Length);

			foreach (var s in resource.Parts)
			{
				if (s.When == null)
				{
					resultingModels.AddRange(s.Apply);
				}
				else if (s.When.Length > 0)
				{
					bool passes = true;
					foreach (var rule in s.When)
					{
						if (!PassesMultiPartRule(rule, blockState))
						{
							passes = false;
							break;
						}
					}

					if (passes)
					{
						resultingModels.AddRange(s.Apply);
					}
				}
			}

			return resultingModels.ToArray();
		}

		public static BlockStateModel[] GetBlockStateModels(IWorld world, Vector3 position, IBlockState blockState, BlockStateResource blockStateModel)
		{
			List<BlockStateModel> resultingModels = new List<BlockStateModel>(blockStateModel.Parts.Length);

			foreach (var s in blockStateModel.Parts)
			{
				if (s.When == null)
				{
					resultingModels.AddRange(s.Apply);
				}
				else if (s.When.Length > 0)
				{
					bool passes = true;
					foreach (var rule in s.When)
					{
						if (!PassesMultiPartRule(world, position, rule, blockState))
						{
							passes = false;
							break;
						}
					}

					if (passes)
					{
						resultingModels.AddRange(s.Apply);
					}
				}
			}

			return resultingModels.ToArray();
		}

		private static bool PassesMultiPartRule(MultiPartRule rule, IBlockState blockState)
		{
			if (rule.HasOrContition)
			{
				return rule.Or.Any(o => PassesMultiPartRule(o, blockState));
			}

			if (rule.HasAndContition)
			{
				return rule.And.All(o => PassesMultiPartRule(o, blockState));
			}

			if (CheckRequirements(blockState, "down", rule.Down)
			    && CheckRequirements(blockState, "up", rule.Up)
			    && CheckRequirements(blockState, "north", rule.North)
			    && CheckRequirements(blockState, "east", rule.East)
			    && CheckRequirements(blockState, "south", rule.South)
			    && CheckRequirements(blockState, "west", rule.West))
			{
				return true;
			}

			return false;
		}

		private static bool CheckRequirements(IBlockState baseblockState, string rule, string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return true;

			if (baseblockState.TryGetValue(rule, out string stateValue))
			{
				if (stateValue.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private static bool PassesMultiPartRule(IWorld world, Vector3 position, MultiPartRule rule, IBlockState baseBlock)
		{
			if (rule.HasOrContition)
			{
				return rule.Or.Any(o => PassesMultiPartRule(world, position, o, baseBlock));
			}

			if (rule.HasAndContition)
			{
				return rule.And.All(o => PassesMultiPartRule(world, position, o, baseBlock));
			}

			if (Passes(world, position, baseBlock, "down", rule.Down)
				&& Passes(world, position, baseBlock, "up", rule.Up)
				&& Passes(world, position, baseBlock, "north", rule.North)
				&& Passes(world, position, baseBlock, "east", rule.East)
				&& Passes(world, position, baseBlock, "south", rule.South)
				&& Passes(world, position, baseBlock, "west", rule.West))
			{
				return true;
			}

			return false;
		}

		private static bool Passes(IWorld world, Vector3 position, IBlockState baseblockState, string rule, string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return true;

			Vector3 direction;
			switch (rule)
			{
				case "north":
					direction = Vector3.Forward;
					break;
				case "east":
					direction = Vector3.Right;
					break;
				case "south":
					direction = Vector3.Backward;
					break;
				case "west":
					direction = Vector3.Left;
					break;
				case "up":
					direction = Vector3.Up;
					break;
				case "down":
					direction = Vector3.Down;
					break;
				default:
					direction = Vector3.Zero;
					break;
			}

			var newPos = new BlockCoordinates(position + direction);
			var blockState = world.GetBlockState(newPos);
			var block = blockState.Block;

			var canAttach = block.Solid && (block.IsFullCube || (blockState.Name.Equals(baseblockState.Name, StringComparison.InvariantCultureIgnoreCase)));

			if (value == "true")
			{
				return canAttach;
			}
			else if (value == "false")
			{
				return !canAttach;
			}
			else if (value == "none")
			{
				return block.BlockMaterial == Material.Air;
			}

			return false;
		}
	}
}