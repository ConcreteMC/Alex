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

			level = 0;
			return true;
		}

		public override void GetVertices(IBlockAccess blockAccess, ChunkData chunkBuilder, BlockCoordinates blockCoordinates, Vector3 vectorPos, BlockState baseBlock)
		{
			var position = new BlockCoordinates(vectorPos);
			var blocksUp = blockAccess.GetBlockStates(position.X, position.Y + 1, position.Z).ToArray();//.GetType();
			
			/*
			 * (IsWater && blocksUp.Any(
					x => x.State.Block.Renderable && (x.State.Block.BlockMaterial == Material.Water || x.State.Block.BlockMaterial == Material.WaterPlant))) || (IsLava
					&& blocksUp.Any(x => x.State.Block.Renderable && x.State.Block.BlockMaterial == Material.Lava));
			 */
			bool aboveIsLiquid = blocksUp.Any(x => x.State?.VariantMapper.Model is LiquidBlockModel);

			List<BlockFace> renderedFaces = new List<BlockFace>(6);
			foreach (var face in Faces)
			{
				var pos = position + face.GetBlockCoordinates();

				bool shouldRenderFace = true;
				foreach (var blockState in blockAccess.GetBlockStates(pos.X, pos.Y, pos.Z))
				{
					if (blockState.Storage != 0 && (blockState.State == null || (blockState.State.Block is Air)))
						continue;
					
					var neighbor = blockState.State.Block;
					
					shouldRenderFace = baseBlock.Block.ShouldRenderFace(face, neighbor);
				}
				
				if (shouldRenderFace)
					renderedFaces.Add(face);
			}

			if (renderedFaces.Count == 0 && !aboveIsLiquid)
				return;
			
			int tl , tr, bl, br;
			
			int lowestFound = 999;
			BlockCoordinates lowestBlock = BlockCoordinates.Up;

			bool isFlowing = false;
			int  rot       = 0;

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
				tl = GetAverageLiquidLevels(blockAccess, position, out lowestBlock, out lowestFound);

				tr = GetAverageLiquidLevels(blockAccess, position + BlockCoordinates.Right, out var trl, out var trv);
				if (trv < lowestFound)
				{
					lowestBlock = trl;
					lowestFound = trv;
				}

				bl = GetAverageLiquidLevels(blockAccess, position + BlockCoordinates.Forwards, out var bll, out var blv);
				if (blv < lowestFound)
				{
					lowestBlock = bll;
					lowestFound = blv;
				}

				br = GetAverageLiquidLevels(blockAccess, position + new BlockCoordinates(1, 0, 1), out var brl,
					out var brv);
			}

			double lowestDistance = 9999;

			isFlowing = isFlowing || (tl < 7 || tr < 7 || bl < 7 || br < 7);
			
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

			BlockTextureData map = GetTextureUVMap(Alex.Instance.Resources, texture, 0, 16, 0, 16, 0, Color.White);

			var originalMap = new BlockTextureData(map.TextureInfo, map.TopLeft, map.TopRight, map.BottomLeft, map.BottomRight, map.ColorLeft, map.ColorTop, map.ColorBottom);
			map.Rotate(rot);

			foreach (var face in renderedFaces)
			{
				float s = 1f - Scale;
				var start = Vector3.One * s;
				var end = Vector3.One * Scale;
				
				var faceMap = map;

				if (face != BlockFace.Up)
				{
					faceMap = originalMap;
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

				var   skyLight = blockAccess.GetSkyLight(position + face.GetBlockCoordinates());
				var   blockLight = blockAccess.GetBlockLight(position + face.GetBlockCoordinates());
				
				float height   = 0;
				for (var index = 0; index < vertices.Length; index++)
				{
					var vert = vertices[index];
					if (vert.Position.Y > start.Y)
					{
						const float modifier = 2f;
						if (vert.Position.X < end.X && vert.Position.Z < end.Z)
						{
							height = (modifier * (tl));
							rot = 0;
						}
						else if (vert.Position.X > start.X && vert.Position.Z < end.Z)
						{
							height = (modifier * (tr));
							rot = 270;
						}
						else if (vert.Position.X < end.X && vert.Position.Z > start.Z)
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

					vert.SkyLight = skyLight;
					vert.BlockLight = blockLight;
					
					if (IsWater)
					{
						vert.Color = vertColor;
						vert.Face = BlockFace.None;
					}
					else
					{
						vert.BlockLight = baseBlock.Block.LightValue;
						vert.Face = face;
					}
					
					vert.TexCoords += map.TextureInfo.Position;
					vert.TexCoords *= (Vector2.One / map.TextureInfo.AtlasSize);

					chunkBuilder.AddVertex(blockCoordinates, vert.Position, vert.TexCoords, vert.Color, blockLight, skyLight, RenderStage.Liquid);
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
			for (int xx = -1; xx < 1; xx++)
			{
				for (int zz = -1; zz < 1; zz++)
				{
					if (world.GetBlockStates(position.X + xx, position.Y + 1, position.Z + zz).Any(
						x =>
						{
							if (x.State?.VariantMapper.Model is LiquidBlockModel)
								return true;

							return false;
						}))
					{
						return 8;
					}

					foreach (var bs in world.GetBlockStates(position.X + xx, position.Y, position.Z + zz))
					{
						var b = bs.State;
						
						if (!b.Block.Renderable || !(b?.VariantMapper.Model is LiquidBlockModel l))
							continue;

						if (l.IsLava != IsLava) continue;

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

						//break;
					}
				}
			}

			return level;
		}
	}
}
