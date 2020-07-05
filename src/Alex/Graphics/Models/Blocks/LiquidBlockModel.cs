using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class LiquidBlockModel : BlockModel
	{
		private static PropertyInt LEVEL = new PropertyInt("level");
		
		public bool IsLava = false;
		public bool IsWater => !IsLava;

		public LiquidBlockModel()
		{

		}

		private int GetLevel(BlockState state)
		{
			return 7 - (state.GetTypedValue(LEVEL) & 0x7);
		}

		private bool TryGetLevel(BlockState state, out int level)
		{
			level = -1;
			if (state.TryGetValue("level", out string rawLevel))
			{
				if (int.TryParse(rawLevel, out int lvl))
				{
					level =  7 - (lvl & 0x7);

					return true;
				}
			}

			return false;
		}

		private bool CheckFlowing(IBlockAccess world, BlockCoordinates position)
		{
			var forward = world.GetBlockState(position + BlockCoordinates.Forwards);

			if (forward.Model is LiquidBlockModel)
			{
				if (GetLevel(forward) < 7)
					return true;

				if (GetAverageLiquidLevels(world, position + BlockCoordinates.Forwards, out var _, out var _) < 7)
					return true;
			}

			var backward = world.GetBlockState(position + BlockCoordinates.Backwards);

			if (backward.Model is LiquidBlockModel)
			{
				if (GetLevel(backward) < 7)
					return true;

				if (GetAverageLiquidLevels(world, position + BlockCoordinates.Backwards, out var _, out var _) < 7)
					return true;
			}

			var left = world.GetBlockState(position + BlockCoordinates.Left);

			if (left.Model is LiquidBlockModel)
			{
				if (GetLevel(left) < 7)
					return true;

				if (GetAverageLiquidLevels(world, position + BlockCoordinates.Left, out var _, out var _) <7)
					return true;
			}

			var right = world.GetBlockState(position + BlockCoordinates.Right);

			if (right.Model is LiquidBlockModel)
			{
				if (GetLevel(right) < 7)
					return true;

				if (GetAverageLiquidLevels(world, position + BlockCoordinates.Right, out var _, out var _) < 7)
					return true;
			}

			//if (GetAverageLiquidLevels(world, position - BlockCoordinates.Up, out var _, out var _) < 8)
			//	return true;
			//!Solid

			//return (!(forward.Block.Solid && backward.Block.Solid && left.Block.Solid && right.Block.Solid) && (forward.Block.IsReplacible || backward.Block.IsReplacible || left.Block.IsReplacible || right.Block.IsReplacible));

			return false;
		}

		public override VerticesResult GetVertices(IBlockAccess world, Vector3 vectorPos, Block baseBlock)
		{
			//Level = GetLevel(baseBlock.BlockState);
			
			var position = new BlockCoordinates(vectorPos);
			List< BlockShaderVertex> result = new List<BlockShaderVertex>(36);
			var indexResult = new List<int>();

			List<BlockFace> renderedFaces = new List<BlockFace>();
			foreach (var face in Enum.GetValues(typeof(BlockFace)).Cast<BlockFace>())
			{
				var pos = position + face.GetBlockCoordinates();

				bool shouldRenderFace = true;
				foreach (var blockState in world.GetBlockStates(pos.X, pos.Y, pos.Z))
				{
					if (blockState.Storage != 0 && (blockState.State == null || (blockState.State.Block is Air)))
						continue;
					
					shouldRenderFace = baseBlock.ShouldRenderFace(face, blockState.State.Block);
				}
				
				if (shouldRenderFace)
					renderedFaces.Add(face);
			}
			
			if (renderedFaces.Count == 0)
				return new VerticesResult(new BlockShaderVertex[0], new int[0]);
			
			int tl , tr, bl, br;

		//	var myLevel = GetLevel(baseBlock.BlockState);
			
			int lowestFound = 999;
			BlockCoordinates lowestBlock = BlockCoordinates.Up;

			bool isFlowing = CheckFlowing(world, position);;
			int rot = 0;
			bool calculateDirection = false;

			var check = position + BlockCoordinates.Up;
			var blocksUp = world.GetBlockStates(check.X, check.Y, check.Z).ToArray();//.GetType();

			if ((IsWater && blocksUp.Any(
				    x => x.State.Block.Renderable && x.State.Block.BlockMaterial == Material.Water))
			    || (IsLava && blocksUp.Any(
				    x => x.State.Block.Renderable && x.State.Block.BlockMaterial == Material.Lava))
			)
			{
				tl = 8;
				tr = 8;
				bl = 8;
				br = 8;

				rot = 180;
				isFlowing = true;
			}
			else
			{
				if (isFlowing)
				{
					calculateDirection = true;
				}
				else
				{
					/*if (blocksUp.Any(x => x.State.Block.Solid && x.State.Block.Renderable))
					{
						tl = 8;
						tr = 8;
						bl = 8;
						br = 8;
					}
					else
					{
						tl = 7;
						tr = 7;
						bl = 7;
						br = 7;
					}*/
				}
				
				tl = GetAverageLiquidLevels(world, position, out lowestBlock, out lowestFound);

				tr = GetAverageLiquidLevels(world, position + BlockCoordinates.Right, out var trl, out var trv);
				if (trv > lowestFound)
				{
					lowestBlock = trl;
					lowestFound = trv;
				}

				bl = GetAverageLiquidLevels(world, position + BlockCoordinates.Forwards, out var bll, out var blv);
				if (blv > lowestFound)
				{
					lowestBlock = bll;
					lowestFound = blv;
				}

				br = GetAverageLiquidLevels(world, position + new BlockCoordinates(1, 0, 1), out var brl,
					out var brv);

				//if (brv < lowestFound)
				//	lowestBlock = brl;
			}

			double lowestDistance = 9999;

			if (lowestBlock.Y <= position.Y && calculateDirection)
			{
				for (int i = 0; i < 4; i++)
				{
					var rotation = 0;
					BlockCoordinates offset = BlockCoordinates.Zero;
					switch (i)
					{
						case 0:
							offset = BlockCoordinates.North;
							rotation = 180;
							break;
						case 1:
							offset = BlockCoordinates.South;
							rotation = 0;
							break;
						case 2:
							offset = BlockCoordinates.East;
							rotation = 270;
							break;
						case 3:
							offset = BlockCoordinates.West;
							rotation = 90;
							break;
					}

					var distance = Math.Abs(
						(position + offset).DistanceTo(lowestBlock));
					if (distance < lowestDistance)
					{
						lowestDistance = distance;
						rot = rotation;
					}
				}
			}

			string texture = "";
			if (IsLava)
			{
				texture = "block/lava";
			}
			else
			{
				texture = "block/water";
			}
			//texture = texture + "_flow";
			if (isFlowing)
			{
				texture += "_flow";
			}
			else
			{
				texture += "_still";
			}

			//float frameX 
			UVMap map = GetTextureUVMap(baseBlock, Alex.Instance.Resources, texture, 0, 16, 0, 16, 0, Color.White);

			var originalMap = new UVMap(map.TopLeft, map.TopRight, map.BottomLeft, map.BottomRight, map.ColorLeft, map.ColorTop, map.ColorBottom);
			//originalMap.Rotate(180);
			
			map.Rotate(rot);

			foreach (var face in renderedFaces)
			{
				float s = 1f - Scale;
				var start = Vector3.One * s;
				var end = Vector3.One * Scale;


				//if (b.BlockModel is LiquidBlockModel m && m.Level > Level && f != BlockFace.Up) continue;

				var faceMap = map;
				if (face != BlockFace.Up)
				{
					faceMap = originalMap;
				}

				var vertices = GetFaceVertices(face, start, end, faceMap, out int[] indexes);

				float height = 0;
				var initialIndex = result.Count;
				for (var index = 0; index < vertices.Length; index++)
				{
					var vert = vertices[index];
					if (vert.Position.Y > start.Y)
					{
						const float modifier = 2f;
						if (vert.Position.X == start.X && vert.Position.Z == start.Z)
						{
							height = (modifier * (tl));
							rot = 0;
						}
						else if (vert.Position.X != start.X && vert.Position.Z == start.Z)
						{
							height = (modifier * (tr));
							rot = 270;
						}
						else if (vert.Position.X == start.X && vert.Position.Z != start.Z)
						{
							height = (modifier * (bl));
							rot = 90;
						}
						else
						{
							height = (modifier * (br));
							rot = 270;
						}

						vert.Position.Y = height / 16.0f; //; + (position.Y);
					}

					vert.Position.Y += position.Y - s;
					vert.Position.X += position.X;
					vert.Position.Z += position.Z;

					if (IsWater)
					{
						vert.Color = new Color(68, 175, 245);
					}
					else
					{
						vert.BlockLight = baseBlock.LightValue;
					}
					//vert.Color = AdjustColor(new Color(68, 175, 245), f,
					//	GetLight(world, position + d), false);

					result.Add(vert);
				}

				for (var index = 0; index < indexes.Length; index++)
				{
					//var vert = vertices[index];
					//var vert = vertices[indexes[index]];
					indexResult.Add(initialIndex + indexes[index]);
				}
			}

			return new VerticesResult(result.ToArray(), indexResult.ToArray());
		}

		protected int GetAverageLiquidLevels(IBlockAccess world, BlockCoordinates position, out BlockCoordinates lowest, out int lowestLevel)
		{
			lowest = BlockCoordinates.Up;
			lowestLevel = 7;
			
			int level = 0;
			for (int xx = -1; xx <= 0; xx++)
			{
				for (int zz = -1; zz <= 0; zz++)
				{
					foreach (var bs in world.GetBlockStates(position.X + xx, position.Y, position.Z + zz))
					{
						var b = bs.State;
						
						if (!b.Block.Renderable || !(b.Model is LiquidBlockModel))
							continue;
						
						if (b.Model is LiquidBlockModel l && l.IsLava == IsLava)
						{
							if (TryGetLevel(b, out int lvl))
							{
								if (lvl > level)
								{
									level = lvl;
								}

								if (lvl < lowestLevel)
								{
									lowestLevel = lvl;
									lowest = new BlockCoordinates(position.X + xx, position.Y, position.Z + zz);
								}
							}

							break;
						}
					}
				}
			}

			return level;
		}
	}
}
