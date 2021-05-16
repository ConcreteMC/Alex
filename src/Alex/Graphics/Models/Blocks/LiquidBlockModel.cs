using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
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
using Microsoft.Xna.Framework.Graphics.PackedVector;

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
			level = 0;
			if (state.TryGetValue("level", out string rawLevel))
			{
				if (int.TryParse(rawLevel, out int lvl))
				{
					level = (lvl);

					return true;
				}
			}

			level = 0;
			return false;
		}

		public override void GetVertices(IBlockAccess blockAccess, ChunkData chunkBuilder, BlockCoordinates blockCoordinates, BlockState baseBlock)
		{
			var position = blockCoordinates;
			var blocksUp = blockAccess.GetBlockStates(position.X, position.Y + 1, position.Z).ToArray();//.GetType();
			
			/*
			 * (IsWater && blocksUp.Any(
					x => x.State.Block.Renderable && (x.State.Block.BlockMaterial == Material.Water || x.State.Block.BlockMaterial == Material.WaterPlant))) || (IsLava
					&& blocksUp.Any(x => x.State.Block.Renderable && x.State.Block.BlockMaterial == Material.Lava));
			 */
			bool aboveIsLiquid = blocksUp.Any(x => x.State?.VariantMapper?.Model is LiquidBlockModel);

			List<BlockFace> renderedFaces = new List<BlockFace>(6);
			foreach (var face in Faces)
			{
				var pos = position + face.GetBlockCoordinates();

				bool shouldRenderFace = false;
				var blockState = blockAccess.GetBlockState(pos);
				{
				//	if (blockState.Storage != 0 && (blockState.State == null || (blockState.State.Block is Air)))
					//	continue;
					
					var neighbor = blockState.Block;
					
					shouldRenderFace = baseBlock.Block.ShouldRenderFace(face, neighbor);
				}
				
				if (shouldRenderFace)
					renderedFaces.Add(face);
			}

			if (renderedFaces.Count == 0 && !aboveIsLiquid)
				return;
			
			int topLeft , topRight, bottomLeft, bottomRight;
			
			int lowestFound = 999;
			BlockCoordinates lowestBlock = BlockCoordinates.Up;

			bool isFlowing = false;
			int  rot       = 0;

			if (aboveIsLiquid)
			{
				topLeft = 8;
				topRight = 8;
				bottomLeft = 8;
				bottomRight = 8;

				rot = 180;
				isFlowing = true;
			}
			else
			{
				topLeft = GetAverageLiquidLevels(blockAccess, position, out lowestBlock, out lowestFound);

				topRight = GetAverageLiquidLevels(blockAccess, position + BlockCoordinates.Right, out var trl, out var trv);
				if (trv < lowestFound)
				{
					lowestBlock = trl;
					lowestFound = trv;
				}

				bottomLeft = GetAverageLiquidLevels(blockAccess, position + BlockCoordinates.Forwards, out var bll, out var blv);
				if (blv < lowestFound)
				{
					lowestBlock = bll;
					lowestFound = blv;
				}

				bottomRight = GetAverageLiquidLevels(blockAccess, position + new BlockCoordinates(1, 0, 1), out var brl,
					out var brv);
			}

			double lowestDistance = 9999;

			isFlowing = isFlowing || (topLeft > 0 || topRight > 0 || bottomLeft > 0 || bottomRight > 0);
			
			if (lowestBlock.Y <= position.Y && isFlowing)
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

			BlockTextureData map = GetTextureUVMap(Alex.Instance.Resources, texture, 0, 16, 0, 16, rot, Color.White, null);

			//var originalMap = new BlockTextureData(map.TextureInfo, map.TopLeft, map.TopRight, map.BottomLeft, map.BottomRight, map.ColorLeft, map.ColorTop, map.ColorBottom);

			foreach (var face in renderedFaces)
			{
				float s = 1f;
				var start = Vector3.Zero;
				var end = Vector3.One;
				
				var faceMap = map;

				if (face != BlockFace.Up)
				{
					//faceMap = originalMap;
				}

				var   vertices  = GetFaceVertices(face, start, end, faceMap);
				Color vertColor;
				
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

			//	var   skyLight = blockAccess.GetSkyLight(position + face.GetBlockCoordinates());
			//	var   blockLight = blockAccess.GetBlockLight(position + face.GetBlockCoordinates());
				
				float height   = 0;

				for (var index = 0; index < vertices.Length; index++)
				{
					var vert = vertices[index];

					if (vert.Position.Y > start.Y)
					{
						const float modifier = 2f;

						if (vert.Position.X < end.X && vert.Position.Z < end.Z)
						{
							height = (topLeft);
							rot = 0;
						}
						else if (vert.Position.X > start.X && vert.Position.Z < end.Z)
						{
							height = (topRight);
							rot = 270;
						}
						else if (vert.Position.X < end.X && vert.Position.Z > start.Z)
						{
							height = (bottomLeft);
							rot = 90;
						}
						else
						{
							height = (bottomRight);
							rot = 270;
						}

						if (height > 8)
							height -= 8;

						if (height == 0)
							height = 7;
						
						height *= modifier;
						
						vert.Position.Y = MathF.Abs((1f / 16f) * height);
						//if (vert.Position.Y <= 0)
						//vert.Position.Y = height; //; + (position.Y);
					}

					if (IsWater)
					{
						vert.Color = vertColor;
						vert.Face = BlockFace.None;
					}
					else
					{
						//vert.Lighting = new Short2(skyLight, baseBlock.Block.LightValue);

					//	blockLight = baseBlock.Block.LightValue;
						vert.Face = face;
					}

					//var textureCoordinates = map.TextureInfo.Position * (Vector2.One / map.TextureInfo.AtlasSize);
					//vert.TexCoords = new Short2(textureCoordinates);
					
					//vert.TexCoords /= 16f;
					vert.TexCoords += map.TextureInfo.Position;
				//	vert.TexCoords *= (Vector2.One / map.TextureInfo.AtlasSize);

					chunkBuilder.AddVertex(
						blockCoordinates, vert.Position, vert.Face,
						new Vector4(
							vert.TexCoords.X, vert.TexCoords.Y, 
							 map.TextureInfo.Width,
							map.TextureInfo.Height), vert.Color,
						RenderStage.Liquid);
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
			lowestLevel =  7;

			int blocks = 0;
			int level = 0;
			for (int xx = -1; xx < 1; xx++)
			{
				for (int zz = -1; zz < 1; zz++)
				{
					/*if (world.GetBlockStates(position.X + xx, position.Y + 1, position.Z + zz).Any(
						x =>
						{
							if (x.State?.VariantMapper.Model is LiquidBlockModel)
								return true;

							return false;
						}))
					{
						return 8;
					}*/

					foreach (var bs in world.GetBlockStates(position.X + xx, position.Y, position.Z + zz))
					{
						var b = bs.State;
						
						if (!(b?.VariantMapper.Model is LiquidBlockModel l))
							continue;

						if (l.IsLava != IsLava) continue;

						if (TryGetLevel(b, out int lvl))
						{
							blocks++;

							level += lvl;
							//if (lvl > level)
							//{
							//	level = lvl;
							//}

							if (lvl > lowestLevel)
							{
								lowestLevel = lvl;
								lowest = new BlockCoordinates(position.X + xx, position.Y, position.Z + zz);
							}
						}

						//break;
					}
				}
			}

			if (level > 0 && blocks > 0)
			{
				level /= blocks;
			}

			return level;
			
			var result = (blocks > 0 ? level / blocks : 0);

			return result;
		}
	}
}
