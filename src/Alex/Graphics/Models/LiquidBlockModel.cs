using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Alex.ResourcePackLib.Json;

namespace Alex.Graphics.Models
{
	public class LiquidBlockModel : BlockModel
	{
		public bool IsLava = false;
		public bool IsFlowing = false;
		public int Level = 8;

		private int _frame { get; set; } = 0;
		public LiquidBlockModel()
		{

		}

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
		{
			List< VertexPositionNormalTextureColor> result = new List<VertexPositionNormalTextureColor>();
			int tl = 0, tr = 0, bl = 0, br = 0;

			int b1, b2;
			if (IsLava)
			{
				b1 = 10;
				b2 = 11;
			}
			else
			{
				b1 = 8;
				b2 = 9;
			}

			var bc = world.GetBlock(position + Vector3.Up).BlockId;//.GetType();
			if (bc == b1 || bc == b2)
			{
				tl = 8;
				tr = 8;
				bl = 8;
				br = 8;
			}
			else
			{
				tl = GetAverageLiquidLevels(world, position, b1, b2);
				tr = GetAverageLiquidLevels(world, position + Vector3.UnitX, b1, b2);
				bl = GetAverageLiquidLevels(world, position + Vector3.UnitZ, b1, b2);
				br = GetAverageLiquidLevels(world, position + new Vector3(1, 0, 1), b1, b2);
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
			UVMap map = GetTextureUVMap(Alex.Instance.Resources, texture, 0, 16, 0, 16);

			foreach (var f in Enum.GetValues(typeof(BlockFace)).Cast<BlockFace>())
			{
				Vector3 d = Vector3.Zero;
				switch (f)
				{
					case BlockFace.Up:
						d = Vector3.Up;
						break;
					case BlockFace.Down:
						d = Vector3.Down;
						break;
					case BlockFace.North:
						d = Vector3.Backward;
						break;
					case BlockFace.South:
						d = Vector3.Forward;
						break;
					case BlockFace.West:
						d = Vector3.Left;
						break;
					case BlockFace.East:
						d = Vector3.Right;
						break;
				}

				float height = 0;
				bool special = f == BlockFace.Up && (tl < 8 || tr < 8 || bl < 8 || br < 8);
				
				var b = (Block)world.GetBlock(position + d);
				LiquidBlockModel m = b.BlockModel as LiquidBlockModel;
				var secondSpecial = m != null && m.Level > Level;

				if (special || (secondSpecial) || !b.BlockId.Equals(b1) && !b.BlockId.Equals(b2))
				{
					//if (b.BlockModel is LiquidBlockModel m && m.Level > Level && f != BlockFace.Up) continue;

					var vertices = GetFaceVertices(f, Vector3.Zero, Vector3.One, map);
					byte cr, cg, cb;
					cr = 255;
					cg = 255;
					cb = 255;

					for (var index = 0; index < vertices.Length; index++)
					{
						var vert = vertices[index];


						if (vert.Position.Y == 0)
						{
							vert.Position.Y = (position.Y);
						}
						else
						{
							if (vert.Position.X == 0 && vert.Position.Z == 0)
							{
								height = ((16.0f / 8.0f) * (tl));
								vert.Position.Y = (height) / 16.0f + (position.Y);
							}
							else if (vert.Position.X != 0 && vert.Position.Z == 0)
							{
								height = ((16.0f / 8.0f) * (tr));
								vert.Position.Y = (height) / 16.0f + (position.Y);
							}
							else if (vert.Position.X == 0 && vert.Position.Z != 0)
							{
								height = ((16.0f / 8.0f) * (bl));
								vert.Position.Y = (height) / 16.0f + (position.Y);
							}
							else
							{
								height = ((16.0f / 8.0f) * (br));
								vert.Position.Y = (height) / 16.0f + (position.Y);
							}
						}


						vert.Position.X += (position.X);
						vert.Position.Z += (position.Z);

						vert.Color = UvMapHelp.AdjustColor(vert.Color, f, GetLight(world, position), false);

						result.Add(vert);
					}
				}
			}

			return result.ToArray();
		}

		protected int GetAverageLiquidLevels(IWorld world, Vector3 position, int b1, int b2)
		{
			int level = 0;
			for (int xx = -1; xx <= 0; xx++)
			{
				for (int zz = -1; zz <= 0; zz++)
				{
					var b = (Block)world.GetBlock(position.X + xx, position.Y + 1, position.Z + zz);
					if ((b.BlockId == b1 || b.BlockId == b2) && b.BlockModel is LiquidBlockModel m && m.IsLava == IsLava)
					{
						return 8;
					}

					b = (Block)world.GetBlock(position.X + xx, position.Y, position.Z + zz);
					if ((b.BlockId == b1 || b.BlockId == b2) && b.BlockModel is LiquidBlockModel l && l.IsLava == IsLava)
					{
						var nl = 7 - (Level & 0x7);
						if (nl > level)
						{
							level = nl;
						}
					}
				}
			}

			return level;
		}


	}
}
