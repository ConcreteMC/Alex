using Alex.API.Blocks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
    public sealed class ItemModelCube : Model
    {
        public Vector3 Size;

        public bool Mirrored { get; set; } = false;

        public ItemModelCube(Vector3 size, Vector3 position)
        {
            this.Size = size;

            //front verts with position and texture stuff
            _topLeftFront = new Vector3(position.X, position.Y + 1.0f, position.Z) * Size;
            _topLeftBack = new Vector3(position.X, position.Y + 1.0f, position.Z + 1.0f) * Size;
            _topRightFront = new Vector3(position.X + 1.0f, position.Y + 1.0f, position.Z) * Size;
            _topRightBack = new Vector3(position.X + 1.0f, position.Y + 1.0f, position.Z + 1.0f) * Size;

            // Calculate the position of the vertices on the bottom face.
            _btmLeftFront = new Vector3(position.X, position.Y, position.Z ) * Size;
            _btmLeftBack = new Vector3(position.X, position.Y, position.Z + 1.0f) * Size;
            _btmRightFront = new Vector3(position.X + 1.0f, position.Y, position.Z ) * Size;
            _btmRightBack = new Vector3(position.X + 1.0f, position.Y, position.Z + 1.0f) * Size;
        }

        public VertexPositionColor[] Front, Back, Left, Right, Top, Bottom;

        private readonly Vector3 _topLeftFront;
        private readonly Vector3 _topLeftBack;
        private readonly Vector3 _topRightFront;
        private readonly Vector3 _topRightBack;
        private readonly Vector3 _btmLeftFront;
        private readonly Vector3 _btmLeftBack;
        private readonly Vector3 _btmRightFront;
        private readonly Vector3 _btmRightBack;

        public void BuildCube(Color color)
        {
            Front = GetFrontVertex(AdjustColor(color, BlockFace.North));
            Back = GetBackVertex(AdjustColor(color, BlockFace.South));
            Left = GetLeftVertex(AdjustColor(color, BlockFace.West));
            Right = GetRightVertex(AdjustColor(color, BlockFace.East));
            Top = GetTopVertex(AdjustColor(color, BlockFace.Up));
            Bottom = GetBottomVertex(AdjustColor(color, BlockFace.Down));
        }
	    
        private VertexPositionColor[] GetLeftVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return (new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftFront, color),
                new VertexPositionColor(_btmLeftFront, color),
                new VertexPositionColor(_btmLeftBack, color),
                new VertexPositionColor(_topLeftBack, color),
                new VertexPositionColor(_topLeftFront, color),
                new VertexPositionColor(_btmLeftBack, color),
            });
        }

        private VertexPositionColor[] GetRightVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return new VertexPositionColor[]
            {
                new VertexPositionColor(_topRightFront, color),
                new VertexPositionColor(_btmRightBack, color),
                new VertexPositionColor(_btmRightFront, color),
                new VertexPositionColor(_topRightBack, color),
                new VertexPositionColor(_btmRightBack , color),
                new VertexPositionColor(_topRightFront, color),
            };
        }

        private VertexPositionColor[] GetFrontVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftFront, color),
                new VertexPositionColor(_topRightFront, color),
                new VertexPositionColor(_btmLeftFront, color),
                new VertexPositionColor(_btmLeftFront , color),
                new VertexPositionColor(_topRightFront, color),
                new VertexPositionColor(_btmRightFront, color),
            };
        }

        private VertexPositionColor[] GetBackVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftBack, color),
                new VertexPositionColor(_btmLeftBack, color),
                new VertexPositionColor(_topRightBack, color),
                new VertexPositionColor(_btmLeftBack , color),
                new VertexPositionColor(_btmRightBack, color),
                new VertexPositionColor(_topRightBack, color),
            };
        }

        private VertexPositionColor[] GetTopVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return new VertexPositionColor[]
            {
                new VertexPositionColor(_topLeftFront, color),
                new VertexPositionColor(_topLeftBack, color),
                new VertexPositionColor(_topRightBack, color),
                new VertexPositionColor(_topLeftFront , color),
                new VertexPositionColor(_topRightBack , color),
                new VertexPositionColor(_topRightFront, color),
            };
        }

        private VertexPositionColor[] GetBottomVertex(Color color)
        {
            // Add the vertices for the RIGHT face. 
            return new VertexPositionColor[]
            {
                new VertexPositionColor(_btmLeftFront, color),
                new VertexPositionColor(_btmRightBack, color),
                new VertexPositionColor(_btmLeftBack, color),
                new VertexPositionColor(_btmLeftFront , color),
                new VertexPositionColor(_btmRightFront, color),
                new VertexPositionColor(_btmRightBack , color),
            };
        }
    }
}