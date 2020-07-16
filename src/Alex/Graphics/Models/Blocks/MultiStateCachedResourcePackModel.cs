using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
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
			/*var clone = blockState.Clone();
			var models = GetBlockStateModels(world, position, blockState, blockStateModel, out Dictionary<string, string> properties);
			clone.AppliedModels = models.Select(x => x.ModelName).ToArray();
			clone.Model = new ResourcePackBlockModel(resourceManager, models);
			clone.Block.BlockState = clone;
			clone.Values = properties;
			//clone.WithPropertyNoResolve()
			return clone;*/
			var blockStateCopy = blockState.VariantMapper.GetDefaultState().CloneSilent();
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
		
		public static BlockStateModel[] GetBlockStateModels(IBlockAccess world, Vector3 position, BlockState blockState, BlockStateResource blockStateModel, out Dictionary<string, string> properties)
		{
			properties = new Dictionary<string, string>();
			List<BlockStateModel> resultingModels = new List<BlockStateModel>(blockStateModel.Parts.Length);
				//		List<MultiPartRule> passedRules = new List<MultiPartRule>();
			
			foreach (var s in blockStateModel.Parts)
			{
				if (s.When == null)
				{
					resultingModels.AddRange(s.Apply);
				}
				else if (s.When.Length > 0)
				{
					List<MultiPartRule> pass = new List<MultiPartRule>();
					
					bool passes = true;
					foreach (var rule in s.When)
					{
						if (!PassesMultiPartRule(world, position, rule, blockState, out var rulePassed))
						{
							passes = false;
							break;
						}
						
						pass.Add(rulePassed);
					}

					if (passes)
					{
						resultingModels.AddRange(s.Apply);

						foreach (var r in pass)
						{
							foreach (var kv in r.KeyValues)
							{
								properties[kv.Key] = kv.Value;
							}
						}
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
			if (baseblockState.Block is IMultipartCheck multipartChecker)
			{
				return multipartChecker.Passes(rule, value);
			}
			
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

		public static bool TryGetBlockface(string value, out BlockFace face)
		{
			switch (value)
			{
				case "north":
					face = BlockFace.South;
					return true;
				case "east":
					face = BlockFace.East;
					return true;
				case "south":
					face = BlockFace.North;
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