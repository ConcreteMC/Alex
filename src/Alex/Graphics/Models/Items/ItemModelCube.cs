using Alex.API.Blocks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
    public sealed class ItemModelCube : Model
    {
        public Vector3 Size;

        public bool Mirrored { get; set; } = false;

        public ItemModelCube(Vector3 size)
        {
            this.Size = size;

            //front verts with position and texture stuff
            _topLeftFront = new Vector3(0.0f, 1.0f, 0.0f) * Size;
            _topLeftBack = new Vector3(0.0f, 1.0f, 1.0f) * Size;
            _topRightFront = new Vector3(1.0f, 1.0f, 0.0f) * Size;
            _topRightBack = new Vector3(1.0f, 1.0f, 1.0f) * Size;

            // Calculate the position of the vertices on the bottom face.
            _btmLeftFront = new Vector3(0.0f, 0.0f, 0.0f) * Size;
            _btmLeftBack = new Vector3(0.0f, 0.0f, 1.0f) * Size;
            _btmRightFront = new Vector3(1.0f, 0.0f, 0.0f) * Size;
            _btmRightBack = new Vector3(1.0f, 0.0f, 1.0f) * Size;
        }

        public (VertexPositionColor[] vertices, short[] indexes) Front, Back, Left, Right, Top, Bottom;

        private readonly Vector3 _topLeftFront;
        private readonly Vector3 _topLeftBack;
        private readonly Vector3 _topRightFront;
        private readonly Vector3 _topRightBack;
        private readonly Vector3 _btmLeftFront;
        private readonly Vector3 _btmLeftBack;
        private readonly Vector3 _btmRightFront;
        private readonly Vector3 _btmRightBack;

        public void BuildCube(Color uv)
        {
            Front = GetFrontVertex(AdjustColor(uv, BlockFace.North));
            Back = GetBackVertex(AdjustColor(uv, BlockFace.South));
            Left = GetLeftVertex(AdjustColor(uv, BlockFace.West));
            Right = GetRightVertex(AdjustColor(uv, BlockFace.East));
            Top = GetTopVertex(AdjustColor(uv, BlockFace.Up));
            Bottom = GetBottomVertex(AdjustColor(uv, BlockFace.Down));
        }
	    
        private (VertexPositionColor[] vertices, short[] indexes) GetLeftVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return (new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftFront, color),
                new VertexPositionColor(_btmLeftFront, color),
                new VertexPositionColor(_btmLeftBack, color),
                new VertexPositionColor(_topLeftBack, color),
                //new VertexPositionNormalTexture(_topLeftFront , normal, map.TopLeft),
                //new VertexPositionNormalTexture(_btmLeftBack, normal, map.BotRight),
            }, new short[]
            {
                0, 1, 2,
                3, 0, 2
                //0, 1, 2, 3, 0, 2
            });
        }

        private (VertexPositionColor[] vertices, short[] indexes) GetRightVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return (new VertexPositionColor[]
            {
                new VertexPositionColor(_topRightFront, color),
                new VertexPositionColor(_btmRightBack, color),
                new VertexPositionColor(_btmRightFront, color),
                new VertexPositionColor(_topRightBack, color),
                //new VertexPositionNormalTexture(_btmRightBack , normal, map.BotLeft),
                //new VertexPositionNormalTexture(_topRightFront, normal, map.TopRight),
            }, new short[]
            {
                0, 1, 2,
                3, 1, 0
            });
        }

        private (VertexPositionColor[] vertices, short[] indexes) GetFrontVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return (new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftFront, color),
                new VertexPositionColor(_topRightFront, color),
                new VertexPositionColor(_btmLeftFront, color),
                //new VertexPositionNormalTexture(_btmLeftFront , color),
                //new VertexPositionNormalTexture(_topRightFront, color),
                new VertexPositionColor(_btmRightFront, color),
            }, new short[]
            {
                0, 1, 2,
                2, 1, 3
                //0, 2, 1, 2, 3, 1
            });
        }

        private (VertexPositionColor[] vertices, short[] indexes) GetBackVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return (new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftBack, color),
                new VertexPositionColor(_btmLeftBack, color),
                new VertexPositionColor(_topRightBack, color),
                //new VertexPositionNormalTexture(_btmLeftBack , color),
                new VertexPositionColor(_btmRightBack, color),
                //new VertexPositionNormalTexture(_topRightBack, color),
            }, new short[]
            {
                0, 1, 2,
                1, 3, 2
                //0, 1, 2, 1, 3, 2
            });
        }

        private (VertexPositionColor[] vertices, short[] indexes) GetTopVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return (new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftFront, color),
                new VertexPositionColor(_topLeftBack, color),
                new VertexPositionColor(_topRightBack, color),
                //new VertexPositionNormalTexture(_topLeftFront , color),
                //	new VertexPositionNormalTexture(_topRightBack , color),
                new VertexPositionColor(_topRightFront, color),
            }, new short[]
            {
                0, 1, 2,
                0, 2, 3
            });
        }

        private (VertexPositionColor[] vertices, short[] indexes) GetBottomVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return (new VertexPositionColor[]
            {
                new VertexPositionColor(_btmLeftFront, color),
                new VertexPositionColor(_btmRightBack, color),
                new VertexPositionColor(_btmLeftBack, color),
                //new VertexPositionNormalTexture(_btmLeftFront , color),
                new VertexPositionColor(_btmRightFront, color),
                //new VertexPositionNormalTexture(_btmRightBack , color),
            }, new short[]
            {
                0, 1, 2,
                0, 3, 1
            });
        }
    }
}