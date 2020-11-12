using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API;
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
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class LiquidBlockModel : BlockModel
	{
		private static readonly BlockFace[] Faces = Enum.GetValues(typeof(BlockFace)).Cast<BlockFace>().ToArray();
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

		public override void GetVertices(IBlockAccess blockAccess, ChunkData chunkBuilder, Vector3 vectorPos, Block baseBlock)
		{
			//int myLevel = 
			//var chunk = world.GetChunk(vectorPos);
			//Level = GetLevel(baseBlock.BlockState);
			
			var                      position    = new BlockCoordinates(vectorPos);
			//var                      biome   = world.GetBiome(position);
			List< BlockShaderVertex> result      = new List<BlockShaderVertex>(36);
			var                      indexResult = new List<int>();
			
			var blocksUp = blockAccess.GetBlockStates(position.X, position.Y + 1, position.Z).ToArray();//.GetType();
			
			bool aboveIsLiquid =
				(IsWater && blocksUp.Any(
					x => x.State.Block.Renderable && (x.State.Block.BlockMaterial == Material.Water || x.State.Block.BlockMaterial == Material.WaterPlant))) || (IsLava
					&& blocksUp.Any(x => x.State.Block.Renderable && x.State.Block.BlockMaterial == Material.Lava));

			List<BlockFace> renderedFaces = new List<BlockFace>();
			//List<BlockFace> liquidFaces = new List<BlockFace>();
			foreach (var face in Faces)
			{
				var pos = position + face.GetBlockCoordinates();

				bool shouldRenderFace = true;
				foreach (var blockState in blockAccess.GetBlockStates(pos.X, pos.Y, pos.Z))
				{
					if (blockState.Storage != 0 && (blockState.State == null || (blockState.State.Block is Air)))
						continue;
					
					var neighbor = blockState.State.Block;
					
					shouldRenderFace = baseBlock.ShouldRenderFace(face, neighbor);

				//	if ((neighbor.BlockMaterial == Material.Water || neighbor.BlockMaterial == Material.WaterPlant) && IsWater)
					//{
				//		if (face != BlockFace.Up && face != BlockFace.Down)
				//			liquidFaces.Add(face);
				//	}
				}
				
				if (shouldRenderFace)
					renderedFaces.Add(face);
			}

			if (renderedFaces.Count == 0 && !aboveIsLiquid)
				return;// new VerticesResult(new BlockShaderVertex[0], new int[0]);
			
			int tl , tr, bl, br;

		//	var myLevel = GetLevel(baseBlock.BlockState);
			
			int lowestFound = 999;
			BlockCoordinates lowestBlock = BlockCoordinates.Up;

			bool isFlowing          = CheckFlowing(blockAccess, position);;
			int  rot                = 0;
			bool calculateDirection = false;

			//var avgLiquidLevel = GetAverageLiquidLevels(world, position, out lowestBlock, out lowestFound);
			
			if (aboveIsLiquid)
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

				tl = GetAverageLiquidLevels(blockAccess, position, out lowestBlock, out lowestFound);;
				//tl = GetAverageLiquidLevels(world, position, out lowestBlock, out lowestFound);

				tr = GetAverageLiquidLevels(blockAccess, position + BlockCoordinates.Right, out var trl, out var trv);
				if (trv > lowestFound)
				{
					lowestBlock = trl;
					lowestFound = trv;
				}

				bl = GetAverageLiquidLevels(blockAccess, position + BlockCoordinates.Forwards, out var bll, out var blv);
				if (blv > lowestFound)
				{
					lowestBlock = bll;
					lowestFound = blv;
				}

				br = GetAverageLiquidLevels(blockAccess, position + new BlockCoordinates(1, 0, 1), out var brl,
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

			BlockTextureData map = GetTextureUVMap(Alex.Instance.Resources, texture, 0, 16, 0, 16, 0, Color.White);

			var originalMap = new BlockTextureData(map.TextureInfo, map.TopLeft, map.TopRight, map.BottomLeft, map.BottomRight, map.ColorLeft, map.ColorTop, map.ColorBottom);
			map.Rotate(rot);

			foreach (var face in renderedFaces)
			{
				/*if (!renderedFaces.Contains(face))
				{
					
					continue;
				}*/
				
				float s = 1f - Scale;
				var start = Vector3.One * s;
				var end = Vector3.One * Scale;
				
				var faceMap = map;
				if (face != BlockFace.Up)
				{
					faceMap = originalMap;
				}

				var   vertices  = GetFaceVertices(face, start, end, faceMap, out int[] indexes);
				Color vertColor = Color.White;
				
				var   bx        = position.X;
				var   y         = position.Y;
				var   bz        = position.Z;

				if (ResourcePackBlockModel.SmoothLighting)
				{
					vertColor = CombineColors(
						GetBiomeColor(blockAccess, bx, y, bz), GetBiomeColor(blockAccess,bx - 1, y, bz - 1), GetBiomeColor(blockAccess,bx - 1, y, bz),
						GetBiomeColor(blockAccess,bx, y, bz - 1), GetBiomeColor(blockAccess,bx + 1, y, bz + 1), GetBiomeColor(blockAccess,bx + 1, y, bz),
						GetBiomeColor(blockAccess,bx, y, bz + 1), GetBiomeColor(blockAccess,bx - 1, y, bz + 1), GetBiomeColor(blockAccess,bx + 1, y, bz - 1));
				}
				else
				{
					vertColor = GetBiomeColor(blockAccess, bx, y, bz);
				}
				
				float height       = 0;
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
						vert.Color = vertColor;
						vert.Face = BlockFace.None;
					}
					else
					{
						vert.BlockLight = baseBlock.LightValue;
						vert.Face = face;
					}
					
					vert.TexCoords += map.TextureInfo.Position;
					vert.TexCoords *= (Vector2.One / map.TextureInfo.AtlasSize);

					result.Add(vert);
				}
				
				for (int i = 0; i < indexes.Length; i++)
				{
					var vertex      = result[indexes[i]];
					int vertexIndex = chunkBuilder.AddVertex(position, vertex);

					indexes[i] = vertexIndex;
				}

				for (var idx = 0; idx < indexes.Length; idx++)
				{
					var idxx = indexes[idx];

					chunkBuilder.AddIndex(position, RenderStage.Animated, idxx);
					//animatedIndexResult.Add(initialIndex + idxx);
				}
			}

			//return new VerticesResult(result.ToArray(), indexResult.ToArray());
		}
		
		private Color GetBiomeColor(IBlockAccess access, int x, int y, int z)
		{
			return access.GetBiome(new BlockCoordinates(x, y, z)).Water;
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
