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

		public LiquidBlockModel()
		{

		}

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
		{
			List< VertexPositionNormalTextureColor> result = new List<VertexPositionNormalTextureColor>();
			int tl = 0, tr = 0, bl = 0, br = 0;

			Type b1, b2;
			if (IsLava)
			{
				b1 = typeof(Lava);
				b2 = typeof(FlowingLava);
			}
			else
			{
				b1 = typeof(Water);
				b2 = typeof(FlowingWater);
			}

			var bc = world.GetBlock(position + Vector3.Up).GetType();
			if (bc == b1 || bc == b2)
			{
				tl = 8;
				tr = 8;
				bl = 8;
				br = 8;
			}
			else
			{
				tl = GetAverageLiquidLevels(world, position);
				tr = GetAverageLiquidLevels(world, position + Vector3.UnitX);
				bl = GetAverageLiquidLevels(world, position + Vector3.UnitZ);
				br = GetAverageLiquidLevels(world, position + new Vector3(1, 0, 1));
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

			if (IsFlowing)
			{
				texture = texture + "_flow";
			}
			else
			{
				texture = texture + "_still";
			}

			UVMap map = GetTextureMap(Alex.Instance.Resources, texture, 0, 8, 0, 8);

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
				var b = world.GetBlock(position + d);
				if (special || (!(b.GetType() == b1) && !(b.GetType() == b2)))
				{
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
								height =  ((16.0f / 8.0f) * (tl));
								vert.Position.Y = (height) / 16.0f + (position.Y);
							}
							else if (vert.Position.X != 0 && vert.Position.Z == 0)
							{
								height =  ((16.0f / 8.0f) * (tr));
								vert.Position.Y = (height) / 16.0f + (position.Y);
							}
							else if (vert.Position.X == 0 && vert.Position.Z != 0)
							{
								height =  ((16.0f / 8.0f) * (bl));
								vert.Position.Y = (height) / 16.0f + (position.Y);
							}
							else
							{
								height =  ((16.0f / 8.0f) * (br));
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

		protected int GetAverageLiquidLevels(IWorld world, Vector3 position)
		{
			int level = 0;
			for (int xx = -1; xx <= 0; xx++)
			{
				for (int zz = -1; zz <= 0; zz++)
				{
					var b = (Block)world.GetBlock(position.X + xx, position.Y + 1, position.Z + zz);
					if (b.BlockModel is LiquidBlockModel m && m.IsLava == IsLava)
					{
						return 8;
					}

					b = (Block)world.GetBlock(position.X + xx, position.Y, position.Z + zz);
					if (b.BlockModel is LiquidBlockModel l && l.IsLava == IsLava)
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

		private UVMap GetTextureMap(ResourceManager resources, string texture, float x1, float x2, float y1, float y2)
		{
			if (resources == null)
			{
				x1 = 0;
				x2 = 1 / 32f;
				y1 = 0;
				y2 = 1 / 32f;

				return new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
					new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
					new Microsoft.Xna.Framework.Vector2(x2, y2), Color.White, Color.White, Color.White);
			}

			var textureInfo = resources.Atlas.GetAtlasLocation(texture.Replace("blocks/", ""));
			var textureLocation = textureInfo.Position;

			var uvSize = resources.Atlas.AtlasSize;

			var pixelSizeX = (textureInfo.Width / uvSize.X) / 16f; //0.0625
			var pixelSizeY = (textureInfo.Height / uvSize.Y) / 16f;

			textureLocation.X /= uvSize.X;
			textureLocation.Y /= uvSize.Y;

			x1 = textureLocation.X + (x1 * pixelSizeX);
			x2 = textureLocation.X + (x2 * pixelSizeX);
			y1 = textureLocation.Y + (y1 * pixelSizeY);
			y2 = textureLocation.Y + (y2 * pixelSizeY);


			return new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
				new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
				new Microsoft.Xna.Framework.Vector2(x2, y2), Color.White, Color.White, Color.White);
		}
	}
}
