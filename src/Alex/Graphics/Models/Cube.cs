using System.Collections.Generic;
using Alex.Graphics.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
    public class Cube : Model
    {
        //private const float _size = 1f;
        private VertexPositionNormalTextureColor _topLeft, _topRight, _bottomLeft, _bottomRight;

        public override VertexPositionNormalTextureColor[] GetShape(Vector3 position, Block baseBlock)
        {
            return FullCube(position, baseBlock);
        }

        private VertexPositionNormalTextureColor[] FullCube(Vector3 position, Block baseBlock)
        {
            var top = baseBlock.CreateUVMapping(TextureSide.Top);
            var side = baseBlock.CreateUVMapping(TextureSide.Side);
            var bottom = baseBlock.CreateUVMapping(TextureSide.Bottom);

            var verts = new List<VertexPositionNormalTextureColor>();

	        if (!Alex.Instance.World.IsSolid(position + Vector3.Up) || Alex.Instance.World.IsTransparent(position + Vector3.Up))
	        {
		        verts.AddRange(Top(position, top));
	        }

	        if (!Alex.Instance.World.IsSolid(position + Vector3.Down) || Alex.Instance.World.IsTransparent(position + Vector3.Down))
	        {
		        verts.AddRange(Bottom(position, bottom));
	        }

	        if (!Alex.Instance.World.IsSolid(position + Vector3.Forward) || Alex.Instance.World.IsTransparent(position + Vector3.Forward))
	        {
		        verts.AddRange(Front(position, side));
	        }

	        if (!Alex.Instance.World.IsSolid(position + Vector3.Backward) || Alex.Instance.World.IsTransparent(position + Vector3.Backward))
	        {
		        verts.AddRange(Back(position, side));
	        }

	        if (!Alex.Instance.World.IsSolid(position + Vector3.Left) || Alex.Instance.World.IsTransparent(position + Vector3.Left))
	        {
		        verts.AddRange(Right(position, side));
	        }
			
	        if (!Alex.Instance.World.IsSolid(position + Vector3.Right) || Alex.Instance.World.IsTransparent(position + Vector3.Right))
	        {
		        verts.AddRange(Left(position, side));
	        }
	        return verts.ToArray();
        }

        private VertexPositionNormalTextureColor[] Top(Vector3 position, UVMap uvmap)
        {
            position.X = -position.X;
            position.Y += Size.Y;

            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Up, uvmap.TopLeft, uvmap.ColorTop);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Up, uvmap.TopRight, uvmap.ColorTop);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Up, uvmap.BottomLeft, uvmap.ColorTop);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Up, uvmap.BottomRight, uvmap.ColorTop);

            _topLeft.Position += new Vector3(0, 0, Size.Z);
            _topRight.Position += new Vector3(-Size.X, 0, Size.Z);
            _bottomRight.Position += new Vector3(-Size.X, 0, 0);
            _bottomLeft.Position += new Vector3(0, 0, 0);

            return new[]
            {
                _topLeft, _bottomLeft, _topRight,
                _bottomLeft, _bottomRight, _topRight
            };
        }

        private VertexPositionNormalTextureColor[] Bottom(Vector3 position, UVMap uvmap)
        {
            position.X = -position.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.TopLeft, uvmap.ColorBottom);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.TopRight, uvmap.ColorBottom);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.BottomLeft, uvmap.ColorBottom);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.BottomRight, uvmap.ColorBottom);

            _topLeft.Position += new Vector3(0, 0, Size.Z);
            _topRight.Position += new Vector3(-Size.X, 0, Size.Z);
            _bottomRight.Position += new Vector3(-Size.X, 0, 0);
            _bottomLeft.Position += new Vector3(0, 0, 0);

            return new[]
            {
                _bottomLeft, _topLeft, _topRight,
                _bottomRight, _bottomLeft, _topRight
            };
        }

        private VertexPositionNormalTextureColor[] Front(Vector3 position, UVMap uvmap)
        {
            position.X = -position.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopLeft, uvmap.ColorSide);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopRight, uvmap.ColorSide);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomLeft, uvmap.ColorSide);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomRight, uvmap.ColorSide);

            _topLeft.Position += new Vector3(0, Size.Y, 0);
            _topRight.Position += new Vector3(-Size.X, Size.Y, 0);
            _bottomRight.Position += new Vector3(-Size.X, 0, 0);
            _bottomLeft.Position += new Vector3(0, 0, 0);

            return new[]
            {
                _topLeft, _bottomLeft, _topRight,
                _bottomLeft, _bottomRight, _topRight
            };
        }

        private VertexPositionNormalTextureColor[] Back(Vector3 position, UVMap uvmap)
        {
            position.X = -position.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.TopLeft, uvmap.ColorSide);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.TopRight, uvmap.ColorSide);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.BottomLeft, uvmap.ColorSide);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.BottomRight, uvmap.ColorSide);

            _topLeft.Position += new Vector3(0, Size.Y, Size.Z);
            _topRight.Position += new Vector3(-Size.X, Size.Y, Size.Z);
            _bottomRight.Position += new Vector3(-Size.X, 0, Size.Z);
            _bottomLeft.Position += new Vector3(0, 0, Size.Z);

            return new[]
            {
                _bottomLeft, _topLeft, _topRight,
                _bottomRight, _bottomLeft, _topRight
            };
        }

        private VertexPositionNormalTextureColor[] Right(Vector3 position, UVMap uvmap)
        {
            position.X = -position.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopLeft, uvmap.ColorSide);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopRight, uvmap.ColorSide);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomLeft, uvmap.ColorSide);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomRight, uvmap.ColorSide);

            _topLeft.Position += new Vector3(0, Size.Y, 0);
            _topRight.Position += new Vector3(0, Size.Y, Size.Z);
            _bottomRight.Position += new Vector3(0, 0, Size.Z);
            _bottomLeft.Position += new Vector3(0, 0, 0);

            return new[]
            {
                _bottomLeft, _topLeft, _topRight,
                _bottomRight, _bottomLeft, _topRight
            };
        }

        private VertexPositionNormalTextureColor[] Left(Vector3 position, UVMap uvmap)
        {
            position.X = -position.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopLeft, uvmap.ColorSide);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopRight, uvmap.ColorSide);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomLeft, uvmap.ColorSide);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomRight, uvmap.ColorSide);

            _topLeft.Position += new Vector3(-Size.X, Size.Y, 0);
            _topRight.Position += new Vector3(-Size.X, Size.Y, Size.Z);
            _bottomRight.Position += new Vector3(-Size.X, 0, Size.Z);
            _bottomLeft.Position += new Vector3(-Size.X, 0, 0);

            return new[]
            {
                _topLeft, _bottomLeft, _topRight,
                _bottomLeft, _bottomRight, _topRight
            };
        }
	}
}