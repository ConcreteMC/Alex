using System.Collections.Generic;
using Alex.Graphics.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
    public class Cube : Model
    {
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

            if (!Alex.Instance.World.IsSolid(position + Vector3.Up) ||
                Alex.Instance.World.IsTransparent(position + Vector3.Up))
            {
                verts.AddRange(Top(position, top));
            }

            if (!Alex.Instance.World.IsSolid(position + Vector3.Down) ||
                Alex.Instance.World.IsTransparent(position + Vector3.Down))
            {
                verts.AddRange(Bottom(position, bottom));
            }

            if (!Alex.Instance.World.IsSolid(position + Vector3.Forward) ||
                Alex.Instance.World.IsTransparent(position + Vector3.Forward))
            {
                verts.AddRange(Front(position, side));
            }

            if (!Alex.Instance.World.IsSolid(position + Vector3.Backward) ||
                Alex.Instance.World.IsTransparent(position + Vector3.Backward))
            {
                verts.AddRange(Back(position, side));
            }

            if (!Alex.Instance.World.IsSolid(position + Vector3.Right) ||
                Alex.Instance.World.IsTransparent(position + Vector3.Right))
            {
                verts.AddRange(Right(position, side));
            }

            if (!Alex.Instance.World.IsSolid(position + Vector3.Left) ||
                Alex.Instance.World.IsTransparent(position + Vector3.Left))
            {
                verts.AddRange(Left(position, side));
            }
            return verts.ToArray();
        }

        private VertexPositionNormalTextureColor[] Top(Vector3 position, UVMap uvmap)
        {
            position.X += Size.X;
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
            position.X += Size.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.TopLeft, uvmap.ColorBottom);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.TopRight, uvmap.ColorBottom);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.BottomLeft,
                uvmap.ColorBottom);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Down, uvmap.BottomRight,
                uvmap.ColorBottom);

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
            position.X += Size.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopLeft, uvmap.ColorFront);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopRight, uvmap.ColorFront);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomLeft,
                uvmap.ColorFront);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomRight,
                uvmap.ColorFront);

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
            position.X += Size.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.TopLeft, uvmap.ColorBack);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.TopRight, uvmap.ColorBack);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.BottomLeft,
                uvmap.ColorBack);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Backward, uvmap.BottomRight,
                uvmap.ColorBack);

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
            position.X += Size.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopLeft, uvmap.ColorRight);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopRight, uvmap.ColorRight);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomLeft,
                uvmap.ColorRight);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomRight,
                uvmap.ColorRight);

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
            position.X += Size.X;
            _topLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopLeft, uvmap.ColorLeft);
            _topRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.TopRight, uvmap.ColorLeft);
            _bottomLeft = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomLeft,
                uvmap.ColorLeft);
            _bottomRight = new VertexPositionNormalTextureColor(position, Vector3.Forward, uvmap.BottomRight,
                uvmap.ColorLeft);

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