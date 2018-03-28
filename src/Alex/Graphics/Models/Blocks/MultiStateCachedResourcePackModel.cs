using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.ResourcePackLib.Json.BlockStates;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Graphics.Models.Blocks
{
	public class MultiStateResourcePackModel : ResourcePackModel
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiStateResourcePackModel));
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

		private static bool Passes(IWorld world, Vector3 position, Block baseBlock, string rule, string value)
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

			var block = world.GetBlock(position + direction);
			var canAttach = block.Solid && (block.IsFullCube || block.GetType() == baseBlock.GetType());

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

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
		{
			Vector3 worldPosition = new Vector3(position.X, position.Y, position.Z);

			Variant = GetBlockStateModels(world, worldPosition, baseBlock);
			CalculateBoundingBox();

			return base.GetVertices(world, worldPosition, baseBlock);
		}
	}
}