using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.ResourcePackLib.Json.BlockStates;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Graphics.Models.Blocks
{
	public class MultiPartModels
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiPartModels));
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


		public static BlockState GetBlockState(IBlockAccess world, Vector3 position, BlockState blockState,
			BlockStateResource blockStateModel)
		{
			var blockStateCopy = blockState;
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
								blockStateCopy = blockStateCopy.WithProperty(kv.Key, kv.Value, false);
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
		
		public static BlockStateModel[] GetBlockStateModels(World world, Vector3 position, BlockState blockState, BlockStateResource blockStateModel)
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
						if (!PassesMultiPartRule(world, position, rule, blockState, out _))
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

		public static bool PassesMultiPartRule(IBlockAccess world, Vector3 position, MultiPartRule rule, BlockState baseBlock, out MultiPartRule passedRule)
		{
			MultiPartRule s = rule;
			passedRule = rule;
			
			if (rule.HasOrContition)
			{
				if (rule.Or.Any(o =>
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
				};

				return false;
			}

			if (rule.HasAndContition)
			{
				if (rule.And.All(o =>
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
				};

				return false;
			}

			//return rule.All(x => CheckRequirements(baseBlock, x.Key, x.Value));
			return rule.KeyValues.All(x => Passes(world, position, baseBlock, x.Key, x.Value.ToLower()));
			/*
			if (Passes(world, position, baseBlock, "down", rule.Down)
				&& Passes(world, position, baseBlock, "up", rule.Up)
				&& Passes(world, position, baseBlock, "north", rule.North)
				&& Passes(world, position, baseBlock, "east", rule.East)
				&& Passes(world, position, baseBlock, "south", rule.South)
				&& Passes(world, position, baseBlock, "west", rule.West))
			{
				return true;
			}

			return false;*/
		}

		private static bool Passes(IBlockAccess world, Vector3 position, BlockState baseblockState, string rule,
			string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return true;

			bool isDirection = true;
			BlockFace face = BlockFace.None;
			switch (rule)
			{
				case "north":
					face = BlockFace.South;
					break;
				case "east":
					face = BlockFace.East;
					break;
				case "south":
					face = BlockFace.North;
					break;
				case "west":
					face = BlockFace.West;
					break;
				case "up":
					face = BlockFace.Up;
					break;
				case "down":
					face = BlockFace.Down;
					break;
				default:
					isDirection = false;

					break;
			}

			var direction = face.GetVector3();

			if (isDirection && (value == "true" || value == "false" || value == "none"))
			{
				var newPos     = new BlockCoordinates(position + direction);
				var blockState = world.GetBlockState(newPos);
				var block      = blockState.Block;
				
				if (face == BlockFace.Up && !(block is Air))
					return true;
				//if (face == BlockFace.Up && !(block is Air))
				//	return true;

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
				return val.Equals(value, StringComparison.InvariantCultureIgnoreCase);
			}

			return false;
		}
	}
}