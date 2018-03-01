using System;
using System.Collections.Generic;
using Alex.CoreRT.API.Graphics;
using Alex.CoreRT.API.World;
using Alex.CoreRT.Blocks;
using Alex.CoreRT.Utils;
using Alex.CoreRT.Worlds;
using log4net;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using MiNET.Worlds;
using ResourcePackLib.CoreRT.Json;
using ResourcePackLib.CoreRT.Json.BlockStates;
using Axis = ResourcePackLib.CoreRT.Json.Axis;

namespace Alex.CoreRT.Graphics.Models
{
	public class MultiStateResourcePackModel : ResourcePackModel
	{
		private static ILog Log = LogManager.GetLogger(typeof(MultiStateResourcePackModel));
		static MultiStateResourcePackModel()
		{
			
		}

		private BlockState BlockState { get; }
		public MultiStateResourcePackModel(ResourceManager resources, BlockState blockState) : base(resources)
		{
			BlockState = blockState;	
		}

		private BlockStateModel[] GetBlockStateModels(IWorld world, Vector3 position, Block baseBlock)
		{
			List<BlockStateModel> resultingModels = new List<BlockStateModel>();
			
			foreach (var s in BlockState.Parts)
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
						if (!PassesMultiPartRule(world, position, rule, baseBlock))
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


		private static bool PassesMultiPartRule(IWorld world, Vector3 position, MultiPartRule rule, Block baseBlock)
		{
			if (rule.Or != null && rule.Or.Length > 0)
			{
				foreach (var o in rule.Or)
				{
					if (PassesMultiPartRule(world, position, o, baseBlock))
					{
						return true;
					}
				}
			}

			if (Passes(world, position, baseBlock, "down", rule.Down) 
			    && Passes(world, position, baseBlock, "up", rule.Up) 			                                                          
			    && Passes(world, position, baseBlock, "north", rule.North) 			                                                          
			    && Passes(world, position, baseBlock, "east", rule.East)                     
			    && Passes(world, position, baseBlock, "south", rule.South) 			                                                          
			    && Passes(world, position, baseBlock, "west", rule.West))
			{
				//Log.Info($"Passed tests!");
				return true;
			}

			return false;
		}

		private static bool Passes(IWorld world, Vector3 position, Block baseBlock, string rule, string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return true;

			string opposite = "";
			Vector3 direction;
			switch (rule)
			{
				case "north":
					direction = Vector3.Forward;
					opposite = "south";
					break;
				case "east":
					direction = Vector3.Right;
					opposite = "west";
					break;
				case "south":
					direction = Vector3.Backward;
					opposite = "north";
					break;
				case "west":
					direction = Vector3.Left;
					opposite = "south";
					break;
				case "up":
					direction = Vector3.Up;
					opposite = "down";
					break;
				case "down":
					direction = Vector3.Down;
					opposite = "up";
					break;
				default:
					direction = Vector3.Zero;
				//	Log.Warn($"Unknown rule {rule}");
					break;
			}

			var block = world.GetBlock(position + direction);
			var canAttach = block.Solid && (block.IsFullCube || block.GetType() == baseBlock.GetType());

			if (value == "true")
			{
				return canAttach;
			} 
			//else if (value == "false")
			//{
			//	return !block.Solid;
			//}
			else if (value == "none")
			{
				return block.BlockId == 0;
			}

			return false;
		}

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
		{
			Vector3 worldPosition = new Vector3(position.X, position.Y, position.Z);

			Variant = GetBlockStateModels(world, worldPosition, baseBlock);
			CalculateBoundingBox();

			return base.GetVertices(world, worldPosition, baseBlock);
		}
	}
}