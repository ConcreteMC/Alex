using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
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

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 vectorPos, IBlock baseBlock)
		{
			var position = new BlockCoordinates(vectorPos);
			List< VertexPositionNormalTextureColor> result = new List<VertexPositionNormalTextureColor>(36);
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

			var bc = world.GetBlock(position + BlockCoordinates.Up);//.GetType();
			if ((!IsLava && bc.IsWater) || (IsLava && bc.Name == "minecraft:lava")) //.Name == b1 || bc.Name == b2)
			{
				tl = 8;
				tr = 8;
				bl = 8;
				br = 8;
			}
			else
			{
				tl = GetAverageLiquidLevels(world, position);
				tr = GetAverageLiquidLevels(world, position + BlockCoordinates.Right);
				bl = GetAverageLiquidLevels(world, position + BlockCoordinates.Forwards);
				br = GetAverageLiquidLevels(world, position + new BlockCoordinates(1, 0, 1));
			}

			string texture = "";
			if (IsLava)
			{
				texture = "lava";
			}
			else
			{
				texture = "water";
			}
			texture = texture + "_flow";
			if (IsFlowing)
			{
			//	texture = texture + "_flow";
			}
			else
			{
			//	texture = texture + "_still";
			}

			//float frameX 
			UVMap map = GetTextureUVMap(Alex.Instance.Resources, texture, 0, 16, 0, 16, 0);

			foreach (var f in Enum.GetValues(typeof(BlockFace)).Cast<BlockFace>())
			{
				BlockCoordinates d = BlockCoordinates.Zero;
				switch (f)
				{
					case BlockFace.Up:
						d = BlockCoordinates.Up;
						break;
					case BlockFace.Down:
						d = BlockCoordinates.Down;
						break;
					case BlockFace.North:
						d = BlockCoordinates.Backwards;
						break;
					case BlockFace.South:
						d = BlockCoordinates.Forwards;
						break;
					case BlockFace.West:
						d = BlockCoordinates.Left;
						break;
					case BlockFace.East:
						d = BlockCoordinates.Right;
						break;
				}

				float height = 0;
				bool special = f == BlockFace.Up && (tl < 8 || tr < 8 || bl < 8 || br < 8);

				var modPos = position + d;
				var b = (BlockState)world.GetBlockState(modPos.X, modPos.Y, modPos.Z);
				LiquidBlockModel m = b.Model as LiquidBlockModel;
				var secondSpecial = m != null && m.Level > Level;

				float s = 1f - Scale;
				var start = Vector3.One * s;
				var end = Vector3.One * Scale;

				if (special || (secondSpecial) || (!string.IsNullOrWhiteSpace(b.Name) && (!b.Name.Equals(b1) && !b.Name.Equals(b2))))
				{
					//if (b.BlockModel is LiquidBlockModel m && m.Level > Level && f != BlockFace.Up) continue;

					var vertices = GetFaceVertices(f, start, end, map);
					
					for (var index = 0; index < vertices.Length; index++)
					{
						var vert = vertices[index];

						if (vert.Position.Y > start.Y)
						{
							const float modifier = 2f;
							if (vert.Position.X == start.X && vert.Position.Z == start.Z)
							{
								height = (modifier * (tl));
							}
							else if (vert.Position.X != start.X && vert.Position.Z == start.Z)
							{
								height = (modifier * (tr));
							}
							else if (vert.Position.X == start.X && vert.Position.Z != start.Z)
							{
								height = (modifier * (bl));
							}
							else
							{
								height = (modifier * (br));
							}

							vert.Position.Y = height / 16.0f; //; + (position.Y);
						}

						vert.Position.Y += position.Y - s;
						vert.Position.X += position.X;
						vert.Position.Z += position.Z;

						vert.Color = LightingUtils.AdjustColor(vert.Color, f, GetLight(world, position + d), false);

						result.Add(vert);
					}
				}
			}

			return result.ToArray();
		}

		protected int GetAverageLiquidLevels(IWorld world, BlockCoordinates position)
		{
			int level = 0;
			for (int xx = -1; xx <= 0; xx++)
			{
				for (int zz = -1; zz <= 0; zz++)
				{
					var b = (BlockState)world.GetBlockState(position.X + xx, position.Y + 1, position.Z + zz);
					if ((b.Model is LiquidBlockModel m && m.IsLava == IsLava))
					{
						return 8;
					}

					b = (BlockState)world.GetBlockState(position.X + xx, position.Y, position.Z + zz);
					if ((b.Model is LiquidBlockModel l && l.IsLava == IsLava))
					{
						var nl = 7 - (b.GetTypedValue(LEVEL) & 0x7);
						if (nl > level)
						{
							level = nl;
						}
					}
					else if (b != null && b.GetTypedValue(WATERLOGGED)) //Block is 'waterlogged'
					{
						level = 8;
					}
				}
			}

			return level;
		}


	}
}
