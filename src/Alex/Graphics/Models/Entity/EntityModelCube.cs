using Alex.Common.Blocks;
using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity
{
	public sealed class Cube
	{
		private static readonly Color DefaultColor = Color.White;

		private readonly bool _mirror = false;

		public Cube(EntityModelCube cube, bool mirrored, float inflation, Vector3 originVector)
		{
			_mirror = mirrored;

			var inflatedSize = cube.InflatedSize(inflation);

			var from = originVector; // cube.InflatedOrigin(inflation);

			var to = from + inflatedSize;

			var uv = cube.Uv ?? new EntityModelUV();
			var rawSize = cube.Size;

			//front verts with position and texture stuff
			_topLeftFront = new Vector3(from.X, to.Y, from.Z);
			_topLeftBack = new Vector3(from.X, to.Y, to.Z);

			_topRightFront = new Vector3(to.X, to.Y, from.Z);
			_topRightBack = new Vector3(to.X, to.Y, to.Z);

			// Calculate the position of the vertices on the bottom face.
			_btmLeftFront = new Vector3(from.X, from.Y, from.Z);
			_btmLeftBack = new Vector3(from.X, from.Y, to.Z);
			_btmRightFront = new Vector3(to.X, from.Y, from.Z);
			_btmRightBack = new Vector3(to.X, from.Y, to.Z);

			Front = GetFrontVertex(
				uv.North.WithOptionalSize(rawSize.X, rawSize.Y),
				uv.South.Size.HasValue ? Vector2.Zero : new Vector2(rawSize.Z, rawSize.Z));

			Back = GetBackVertex(
				uv.South.WithOptionalSize(rawSize.X, rawSize.Y),
				uv.North.Size.HasValue ? Vector2.Zero : new Vector2(rawSize.Z + rawSize.Z + rawSize.X, rawSize.Z));

			Left = GetLeftVertex(
				uv.West.WithOptionalSize(rawSize.Z, rawSize.Y),
				uv.West.Size.HasValue ? Vector2.Zero : new Vector2(rawSize.Z + rawSize.X, rawSize.Z));

			Right = GetRightVertex(
				uv.East.WithOptionalSize(rawSize.Z, rawSize.Y),
				uv.East.Size.HasValue ? Vector2.Zero : new Vector2(0, rawSize.Z));

			Top = GetTopVertex(
				uv.Up.WithOptionalSize(rawSize.X, rawSize.Z),
				uv.Up.Size.HasValue ? Vector2.Zero : new Vector2(rawSize.Z, 0));

			Bottom = GetBottomVertex(
				uv.Down.WithOptionalSize(rawSize.X, rawSize.Z),
				uv.Down.Size.HasValue ? Vector2.Zero : new Vector2(rawSize.Z + rawSize.X, 0));
		}

		public (VertexPositionColorTexture[] vertices, short[] indexes) Front, Back, Left, Right, Top, Bottom;

		private readonly Vector3 _topLeftFront;
		private readonly Vector3 _topLeftBack;
		private readonly Vector3 _topRightFront;
		private readonly Vector3 _topRightBack;
		private readonly Vector3 _btmLeftFront;
		private readonly Vector3 _btmLeftBack;
		private readonly Vector3 _btmRightFront;
		private readonly Vector3 _btmRightBack;

		private (VertexPositionColorTexture[] vertices, short[] indexes) GetLeftVertex(EntityModelUVData uv,
			Vector2 size)
		{
			//Vector3 normal = new Vector3(-1.0f, 0.0f, 0.0f) * Size;
			Color normal = Models.ModelBase.AdjustColor(DefaultColor, BlockFace.West);

			//var map = GetTextureMapping(uv + new Vector2(Size.Z + Size.X, Size.Z), Size.Z, Size.Y);
			var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

			// Add the vertices for the RIGHT face. 
			return (new VertexPositionColorTexture[]
			{
				new VertexPositionColorTexture(_topLeftFront, normal, map.TopLeft),
				new VertexPositionColorTexture(_btmLeftFront, normal, map.BotLeft),
				new VertexPositionColorTexture(_btmLeftBack, normal, map.BotRight),
				new VertexPositionColorTexture(_topLeftBack, normal, map.TopRight),
				//new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
				//new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotRight),
			}, new short[]
			{
				0, 1, 2, 3, 0, 2
				//0, 1, 2, 3, 0, 2
			});
		}

		private (VertexPositionColorTexture[] vertices, short[] indexes) GetRightVertex(EntityModelUVData uv,
			Vector2 size)
		{
			//Vector3 normal = new Vector3(1.0f, 0.0f, 0.0f) * Size;
			Color normal = Models.ModelBase.AdjustColor(DefaultColor, BlockFace.East);

			var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);
			//var map = GetTextureMapping(uv + new Vector2(0, Size.Z), Size.Z, Size.Y);

			// Add the vertices for the RIGHT face. 
			return (new VertexPositionColorTexture[]
			{
				new VertexPositionColorTexture(_topRightFront, normal, map.TopRight),
				new VertexPositionColorTexture(_btmRightBack, normal, map.BotLeft),
				new VertexPositionColorTexture(_btmRightFront, normal, map.BotRight),
				new VertexPositionColorTexture(_topRightBack, normal, map.TopLeft),
				//new VertexPositionNormalTexture(_btmRightBack , normal, map.BotLeft),
				//new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
			}, new short[] { 0, 1, 2, 3, 1, 0 });
		}

		private (VertexPositionColorTexture[] vertices, short[] indexes) GetFrontVertex(EntityModelUVData uv,
			Vector2 size)
		{
			//Vector3 normal = new Vector3(0.0f, 0.0f, 1.0f) * Size;
			Color normal = Models.ModelBase.AdjustColor(DefaultColor, BlockFace.South);

			var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

			// Add the vertices for the RIGHT face. 
			return (new VertexPositionColorTexture[]
			{
				new VertexPositionColorTexture(_topLeftFront, normal, map.TopRight),
				new VertexPositionColorTexture(_topRightFront, normal, map.TopLeft),
				new VertexPositionColorTexture(_btmLeftFront, normal, map.BotRight),
				//new VertexPositionNormalTexture(_btmLeftFront , normal, map.BotLeft),
				//new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
				new VertexPositionColorTexture(_btmRightFront, normal, map.BotLeft),
			}, new short[]
			{
				0, 1, 2, 2, 1, 3
				//0, 2, 1, 2, 3, 1
			});
		}

		private (VertexPositionColorTexture[] vertices, short[] indexes) GetBackVertex(EntityModelUVData uv,
			Vector2 size)
		{
			//Vector3 normal = new Vector3(0.0f, 0.0f, -1.0f) * Size;
			Color normal = Models.ModelBase.AdjustColor(DefaultColor, BlockFace.North);

			var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

			// Add the vertices for the RIGHT face. 
			return (new VertexPositionColorTexture[]
			{
				new VertexPositionColorTexture(_topLeftBack, normal, map.TopLeft),
				new VertexPositionColorTexture(_btmLeftBack, normal, map.BotLeft),
				new VertexPositionColorTexture(_topRightBack, normal, map.TopRight),
				//new VertexPositionNormalTexture(_btmLeftBack , normal, map.BotRight),
				new VertexPositionColorTexture(_btmRightBack, normal, map.BotRight),
				//new VertexPositionNormalTexture(_topRightBack, normal, map.TopLeft),
			}, new short[]
			{
				0, 1, 2, 1, 3, 2
				//0, 1, 2, 1, 3, 2
			});
		}

		private (VertexPositionColorTexture[] vertices, short[] indexes) GetTopVertex(EntityModelUVData uv,
			Vector2 size)
		{
			//	Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f) * Size;
			Color normal = Models.ModelBase.AdjustColor(DefaultColor, BlockFace.Up);

			var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

			// Add the vertices for the RIGHT face. 
			return (new VertexPositionColorTexture[]
			{
				new VertexPositionColorTexture(_topLeftFront, normal, map.BotLeft),
				new VertexPositionColorTexture(_topLeftBack, normal, map.TopLeft),
				new VertexPositionColorTexture(_topRightBack, normal, map.TopRight),
				//new VertexPositionNormalTexture(_topLeftFront , normal, map.BotLeft),
				//	new VertexPositionNormalTexture(_topRightBack , normal, map.TopRight),
				new VertexPositionColorTexture(_topRightFront, normal, map.BotRight),
			}, new short[] { 0, 1, 2, 0, 2, 3 });
		}

		private (VertexPositionColorTexture[] vertices, short[] indexes) GetBottomVertex(EntityModelUVData uv,
			Vector2 size)
		{
			//	Vector3 normal = new Vector3(0.0f, -1.0f, 0.0f) * Size;
			Color normal = Models.ModelBase.AdjustColor(DefaultColor, BlockFace.Down);
			var map = GetTextureMapping(uv.Origin + size, uv.Size.Value.X, uv.Size.Value.Y);

			// Add the vertices for the RIGHT face. 
			return (new VertexPositionColorTexture[]
			{
				new VertexPositionColorTexture(_btmLeftFront, normal, map.TopLeft),
				new VertexPositionColorTexture(_btmRightBack, normal, map.BotRight),
				new VertexPositionColorTexture(_btmLeftBack, normal, map.BotLeft),
				//new VertexPositionNormalTexture(_btmLeftFront , normal, map.TopLeft),
				new VertexPositionColorTexture(_btmRightFront, normal, map.TopRight),
				//new VertexPositionNormalTexture(_btmRightBack , normal, map.BotRight),
			}, new short[] { 0, 1, 2, 0, 3, 1 });
		}

		private TextureMapping GetTextureMapping(Vector2 textureOffset, float regionWidth, float regionHeight)
		{
			return new TextureMapping(textureOffset, regionWidth, regionHeight, _mirror);
		}

		private class TextureMapping
		{
			public Vector2 TopLeft { get; }
			public Vector2 TopRight { get; }
			public Vector2 BotLeft { get; }
			public Vector2 BotRight { get; }

			public TextureMapping(Vector2 textureOffset, float width, float height, bool mirrored)
			{
				var x1 = textureOffset.X * 1f;
				var x2 = x1 + (width * 1f);
				var y1 = (textureOffset.Y) * 1f;
				var y2 = y1 + (height * 1f);

				if (mirrored)
				{
					TopLeft = new Vector2(x2, y1);
					TopRight = new Vector2(x1, y1);
					BotLeft = new Vector2(x2, y2);
					BotRight = new Vector2(x1, y2);
				}
				else
				{
					TopLeft = new Vector2(x1, y1);
					TopRight = new Vector2(x2, y1);
					BotLeft = new Vector2(x1, y2);
					BotRight = new Vector2(x2, y2);
				}
			}
		}
	}
}