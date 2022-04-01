using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Blocks;
using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Interfaces;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.Worlds.Abstraction;
using NLog;

namespace Alex.Graphics.Models.Blocks
{
	public static class MultiPartModelHelper
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiPartModelHelper));

		public static BlockStateModel[] GetModels(BlockState blockState, BlockStateResource resource)
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


		public static BlockState GetBlockState(IBlockAccess world,
			BlockCoordinates position,
			BlockState blockState,
			BlockStateResource blockStateModel)
		{
			var blockStateCopy = blockState.VariantMapper.GetDefaultState();

			foreach (var s in blockStateModel.Parts)
			{
				if (s.When == null)
				{
					//resultingModels.AddRange(s.Apply);
				}
				else if (s.When.Length > 0)
				{
					bool passes = true;

					foreach (var rule in s.When)
					{
						if (PassesMultiPartRule(world, position, rule, blockState, out var result))
						{
							foreach (var kv in result.KeyValues)
							{
								blockStateCopy = blockStateCopy.WithProperty(kv.Key, kv.Value);
							}
						}

						passes = false;

						break;
					}

					if (passes)
					{
						//resultingModels.AddRange(s.Apply);
					}
				}
			}

			return blockStateCopy;
		}

		private static bool PassesMultiPartRule(MultiPartRule rule, BlockState blockState)
		{
			if (rule.HasOrContition)
			{
				return rule.Or.Any(o => PassesMultiPartRule(o, blockState));
			}

			if (rule.HasAndContition)
			{
				return rule.And.All(o => PassesMultiPartRule(o, blockState));
			}

			return rule.KeyValues.All(x => CheckRequirements(blockState, x.Key, x.Value));
		}

		private static bool CheckRequirements(BlockState baseblockState, string rule, string value)
		{
			if (baseblockState.Block is IMultipartCheck multipartChecker)
			{
				return multipartChecker.Passes(rule, value);
			}

			if (string.IsNullOrWhiteSpace(value)) return true;

			if (baseblockState.TryGetValue(rule, out string stateValue))
			{
				if (stateValue.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		public static bool PassesMultiPartRule(IBlockAccess world,
			BlockCoordinates position,
			MultiPartRule rule,
			BlockState baseBlock,
			out MultiPartRule passedRule)
		{
			MultiPartRule s = rule;
			passedRule = rule;

			if (rule.HasOrContition)
			{
				if (rule.Or.Any(
					    o =>
					    {
						    var pass = PassesMultiPartRule(world, position, o, baseBlock, out var p);

						    if (pass)
						    {
							    s = p;

							    return true;
						    }

						    return false;
					    }))
				{
					passedRule = s;

					return true;
				}

				;

				return false;
			}

			if (rule.HasAndContition)
			{
				if (rule.And.All(
					    o =>
					    {
						    var pass = PassesMultiPartRule(world, position, o, baseBlock, out var p);

						    if (pass)
						    {
							    s = p;

							    return true;
						    }

						    return false;
					    }))
				{
					passedRule = s;

					return true;
				}

				;

				return false;
			}

			foreach (var x in rule.KeyValues)
			{
				if (!Passes(world, position, baseBlock, x.Key, x.Value.ToLower()))
				{
					return false;
				}
			}

			return true;
		}

		private static bool Passes(IBlockAccess world,
			BlockCoordinates position,
			BlockState baseblockState,
			string rule,
			string value)
		{
			if (baseblockState.Block is IMultipartCheck multipartChecker)
			{
				return multipartChecker.Passes(world, position, rule, value);
			}

			if (string.IsNullOrWhiteSpace(value)) return true;

			bool isDirection = true;

			BlockFace face = BlockFace.None;

			if (!TryGetBlockface(rule, out face))
			{
				isDirection = false;
			}

			var direction = face.GetBlockCoordinates();

			//if (face == BlockFace.North || face == BlockFace.South)
			//	direction = face.Opposite().GetVector3();

			if (isDirection && (value == "true" || value == "false" || value == "none"))
			{
				var newPos = new BlockCoordinates(position + direction);
				var blockState = world.GetBlockState(newPos);
				var block = blockState.Block;

				if (face == BlockFace.Up && !(block is Air))
					return true;

				var canAttach = baseblockState.Block.CanAttach(face, block);

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


			if (baseblockState.TryGetValue(rule, out string val))
			{
				return val.Equals(value, StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}

		public static bool TryGetBlockface(string value, out BlockFace face)
		{
			switch (value)
			{
				case "north":
					face = BlockFace.North;

					return true;

				case "east":
					face = BlockFace.East;

					return true;

				case "south":
					face = BlockFace.South;

					return true;

				case "west":
					face = BlockFace.West;

					return true;

				case "up":
					face = BlockFace.Up;

					return true;

				case "down":
					face = BlockFace.Down;

					return true;

				case "none":
					face = BlockFace.None;

					return true;
			}

			face = BlockFace.None;

			return false;
		}
	}
}