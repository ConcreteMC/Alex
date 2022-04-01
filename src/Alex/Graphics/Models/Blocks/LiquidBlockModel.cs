using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Blocks.Minecraft.Liquid;
using Alex.Blocks.State;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Interfaces;
using Alex.Utils;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
	public class LiquidBlockModel : BlockModel
	{
		private static readonly BlockFace[] Faces = Enum.GetValues(typeof(BlockFace)).Cast<BlockFace>().ToArray();

		public LiquidBlockModel() { }

		private int GetLevel(BlockState state)
		{
			if (state.Block is LiquidBlock lb)
			{
				return LiquidBlock.LEVEL.GetValue(state);
			}

			return -1;
		}

		public override void GetVertices(IBlockAccess blockAccess,
			ChunkData chunkBuilder,
			BlockCoordinates blockCoordinates,
			BlockState baseBlock)
		{
			var position = blockCoordinates;

			var blocksUp =
				blockAccess.GetBlockState(new BlockCoordinates(position.X, position.Y + 1, position.Z)); //.GetType();

			bool aboveIsLiquid = blocksUp.Block is LiquidBlock;

			List<BlockFace> renderedFaces = new List<BlockFace>();

			foreach (var face in Faces)
			{
				var pos = position + face.GetBlockCoordinates();

				bool shouldRenderFace = false;
				var blockState = blockAccess.GetBlockState(pos);

				{
					var neighbor = blockState.Block;

					shouldRenderFace = baseBlock.Block.ShouldRenderFace(face, neighbor);
				}

				if (shouldRenderFace)
					renderedFaces.Add(face);
			}

			if (renderedFaces.Count == 0 && !aboveIsLiquid)
				return;

			int topLeft, topRight, bottomLeft, bottomRight;

			BlockCoordinates lowestBlock = BlockCoordinates.Up;

			bool isFlowing = false;
			int rot = 0;

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
				topLeft = GetAverageLiquidLevels(blockAccess, position);

				topRight = GetAverageLiquidLevels(blockAccess, position + new BlockCoordinates(1, 0, 0));

				bottomLeft = GetAverageLiquidLevels(blockAccess, position + new BlockCoordinates(0, 0, 1));

				bottomRight = GetAverageLiquidLevels(blockAccess, position + new BlockCoordinates(1, 0, 1));
			}

			if (baseBlock.Block is FlowingWater || baseBlock.Block is FlowingLava)
				isFlowing = true;

			double lowestDistance = 9999;

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

					var distance = Math.Abs((position + offset).DistanceTo(lowestBlock));

					if (distance < lowestDistance)
					{
						lowestDistance = distance;
						rot = rotation;
					}
				}
			}

			string texture = "";

			if (baseBlock.Block is Lava)
			{
				texture = "block/lava";
			}
			else
			{
				texture = "block/water";
			}

			if (isFlowing)
			{
				texture += "_flow";
			}
			else
			{
				texture += "_still";
			}

			BlockTextureData map = GetTextureUVMap(
				Alex.Instance.Resources, texture, 0, 16, 0, 16, rot, Color.White, null);

			foreach (var face in renderedFaces)
			{
				var start = Vector3.Zero;
				var end = Vector3.One;

				var faceMap = map;

				var vertices = GetFaceVertices(face, start, end, faceMap);
				Color vertColor;

				var bx = position.X;
				var y = position.Y;
				var bz = position.Z;

				if (ResourcePackBlockModel.SmoothLighting)
				{
					vertColor = CombineColors(
						GetBiomeColor(blockAccess, bx, y, bz), GetBiomeColor(blockAccess, bx - 1, y, bz - 1),
						GetBiomeColor(blockAccess, bx - 1, y, bz), GetBiomeColor(blockAccess, bx, y, bz - 1),
						GetBiomeColor(blockAccess, bx + 1, y, bz + 1), GetBiomeColor(blockAccess, bx + 1, y, bz),
						GetBiomeColor(blockAccess, bx, y, bz + 1), GetBiomeColor(blockAccess, bx - 1, y, bz + 1),
						GetBiomeColor(blockAccess, bx + 1, y, bz - 1));
				}
				else
				{
					vertColor = GetBiomeColor(blockAccess, bx, y, bz);
				}

				for (var index = 0; index < vertices.Length; index++)
				{
					var vert = vertices[index];

					if (vert.Position.Y > start.Y)
					{
						int height = 0;

						if (vert.Position.X < end.X && vert.Position.Z < end.Z)
						{
							height = (int)((16.0 / 8.0) * (topLeft));
						}
						else if (vert.Position.X > start.X && vert.Position.Z < end.Z)
						{
							height = (int)((16.0 / 8.0) * (topRight));
						}
						else if (vert.Position.X < end.X && vert.Position.Z > start.Z)
						{
							height = (int)((16.0 / 8.0) * (bottomLeft));
						}
						else
						{
							height = (int)((16.0 / 8.0) * (bottomRight));
						}

						vert.Position.Y = ((float)height) / 16f;
					}

					if (baseBlock.Block is Water)
					{
						vert.Color = vertColor;
						vert.Face = BlockFace.None;
					}
					else
					{
						vert.Face = face;
					}

					vert.TexCoords += map.TextureInfo.Position;

					chunkBuilder.AddVertex(
						blockCoordinates, vert.Position, vert.Face,
						new Vector4(vert.TexCoords.X, vert.TexCoords.Y, map.TextureInfo.Width, map.TextureInfo.Height),
						vert.Color, RenderStage.Liquid);
				}
			}
		}

		private Color GetBiomeColor(IBlockAccess access, int x, int y, int z)
		{
			return access.GetBiome(new BlockCoordinates(x, y, z)).Water;
		}

		protected int GetAverageLiquidLevels(IBlockAccess world, BlockCoordinates position)
		{
			int level = 0;

			for (int xx = -1; xx < 1; xx++)
			{
				for (int zz = -1; zz < 1; zz++)
				{
					var above = world.GetBlockState(
						new BlockCoordinates(position.X + xx, position.Y + 1, position.Z + zz));

					if (above.Block is LiquidBlock)
						return 8;

					var bs = world.GetBlockState(new BlockCoordinates(position.X + xx, position.Y, position.Z + zz));
					var raw = GetLevel(bs);

					if (raw == -1)
						continue;

					var lvl = 7 - (raw & 0x7);

					if (lvl > level)
						level = lvl;
				}
			}

			return level;
		}
	}
}