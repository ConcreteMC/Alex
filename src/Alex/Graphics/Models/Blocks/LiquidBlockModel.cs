using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class LiquidBlockModel : BlockModel
	{
		private static PropertyInt LEVEL = new PropertyInt("level");
		private static PropertyBool WATERLOGGED = new PropertyBool("waterlogged");

		public bool IsLava = false;
		public bool IsWater => !IsLava;
		public bool IsFlowing = false;
		public int Level = 8;

		public LiquidBlockModel()
		{

		}

		private int GetLevel(IBlockState state)
		{
			return 7 - (state.GetTypedValue(LEVEL) & 0x7);
		}

		private bool Check(IWorld world, BlockCoordinates position)
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

				if (GetAverageLiquidLevels(world, position + BlockCoordinates.Left, out var _, out var _) < 7)
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

		public override (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 vectorPos, IBlock baseBlock)
		{
			var position = new BlockCoordinates(vectorPos);
			List< VertexPositionNormalTextureColor> result = new List<VertexPositionNormalTextureColor>(36);
			var indexResult = new List<int>();

			List<BlockFace> renderedFaces = new List<BlockFace>();
			foreach (var face in Enum.GetValues(typeof(BlockFace)).Cast<BlockFace>())
			{
				if (baseBlock.ShouldRenderFace(face, world.GetBlock(position + face.GetBlockCoordinates())))
				{
					renderedFaces.Add(face);
				}
			}
			
			if (renderedFaces.Count == 0)
				return (new VertexPositionNormalTextureColor[0], new int[0]);
			
			int tl = 0, tr = 0, bl = 0, br = 0;

			Level = baseBlock.BlockState.GetTypedValue(LEVEL);

			string b1, b2;
			if (IsLava)
			{
				b1 = "minecraft:lava";
				b2 = "minecraft:lava";
			}
			else
			{
				b1 = "minecraft:water";
				b2 = "minecraft:water";
			}

			int lowestFound = 999;
			BlockCoordinates lowestBlock = BlockCoordinates.Up;

			bool isFlowing = isFlowing = Check(world, position);;
			int rot = 0;
			bool calculateDirection = false;

			var check = position + BlockCoordinates.Up;
			var bc = world.GetBlockStates(check.X, check.Y, check.Z).ToArray();//.GetType();
			if ((!IsLava && bc.Any(x => x.state.Block.BlockMaterial == Material.Water)) || (IsLava && bc.Any(x => x.state.Block.BlockMaterial == Material.Lava))) //.Name == b1 || bc.Name == b2)
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
					
					tl = GetAverageLiquidLevels(world, position, out lowestBlock, out lowestFound);

					tr = GetAverageLiquidLevels(world, position + BlockCoordinates.Right, out var trl, out var trv);
					if (trv < lowestFound)
					{
						lowestBlock = trl;
						lowestFound = trv;
					}

					bl = GetAverageLiquidLevels(world, position + BlockCoordinates.Forwards, out var bll, out var blv);
					if (blv < lowestFound)
					{
						lowestBlock = bll;
						lowestFound = blv;
					}

					br = GetAverageLiquidLevels(world, position + new BlockCoordinates(1, 0, 1), out var brl,
						out var brv);
				}
				else
				{
					if (bc.Any(x => x.state.Block.Solid && x.state.Block.Renderable))
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
					}
				}

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
			UVMap map = GetTextureUVMap(Alex.Instance.Resources, texture, 0, 16, 0, 16, 0, Color.White);

			var originalMap = new UVMap(map.TopLeft, map.TopRight, map.BottomLeft, map.BottomRight, map.ColorLeft, map.ColorTop, map.ColorBottom);
			//originalMap.Rotate(180);
			
			map.Rotate(rot);
			
			foreach (var face in renderedFaces)
			{
				float height = 0;
				//bool shouldFaceBeRendered = face == BlockFace.Up && (tl < 8 || tr < 8 || bl < 8 || br < 8);

				/*
				if (!shouldFaceBeRendered)
				{
					var modPos = position + face.GetBlockCoordinates();
					var blockStates = world.GetBlockStates(modPos.X, modPos.Y, modPos.Z).Select(x => x.state)
						.Cast<BlockState>().ToArray();

					foreach (var b in blockStates)
					{
						if (!b.Block.Renderable)
							continue;

						LiquidBlockModel m = b.Model as LiquidBlockModel;
						var isSpecial = (m != null && ((m.Level > Level)));
						isSpecial = isSpecial || (face == BlockFace.Up && b.Block.Solid && b.Block.Transparent);

						if (isSpecial)
						{
							shouldFaceBeRendered = true;
							break;
						}
					}
				}*/

				float s = 1f - Scale;
				var start = Vector3.One * s;
				var end = Vector3.One * Scale;

				//if (shouldFaceBeRendered)
				{
					//if (b.BlockModel is LiquidBlockModel m && m.Level > Level && f != BlockFace.Up) continue;

					var faceMap = map;
					if (face != BlockFace.Up)
					{
						faceMap = originalMap;
					}
					
					var vertices = GetFaceVertices(face, start, end, faceMap, out int[] indexes);
					
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

                        if (IsWater) vert.Color = new Color(68, 175, 245);
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
			}

			return (result.ToArray(), indexResult.ToArray());
		}

		protected int GetAverageLiquidLevels(IWorld world, BlockCoordinates position, out BlockCoordinates lowest, out int lowestLevel)
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
						if (!bs.state.Block.Renderable)
							continue;

						if (bs.state.Model is LiquidBlockModel m && m.IsLava != IsLava)
							return 8;
						
						int waterLevel = -1;
						
						var b = bs.state;
						//b = (BlockState) world.GetBlockState(position.X + xx, position.Y, position.Z + zz);
						if ((b.Model is LiquidBlockModel l && l.IsLava == IsLava))
						{
							waterLevel = 7 - (b.GetTypedValue(LEVEL) & 0x7);
						}
						/*else if (b != null && !b.Block.Renderable)
						{
							level = 0;
						}
						else if (b != null && (b.GetTypedValue(WATERLOGGED))) //Block is 'waterlogged'
						{
							waterLevel = 7;
						}*/

						if (waterLevel != -1)
						{
							if (waterLevel > level)
							{
								level = waterLevel;
							}

							if (waterLevel < lowestLevel)
							{
								lowestLevel = waterLevel;
								lowest = new BlockCoordinates(position.X + xx, position.Y, position.Z + zz);
							}
						}
					}
				}
			}

			return level;
		}
	}
}
