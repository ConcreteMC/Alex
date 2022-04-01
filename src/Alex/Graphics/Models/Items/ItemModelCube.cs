using Alex.Common.Blocks;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
	public sealed class ItemModelCube : ModelBase
	{
		public ItemModelCube(Vector3 size, Color color)
		{
			//front verts with position and texture stuff
			_topLeftFront = new Vector3(0f, 1.0f, 0f) * size;
			_topLeftBack = new Vector3(0f, 1.0f, 1.0f) * size;
			_topRightFront = new Vector3(1.0f, 1.0f, 0f) * size;
			_topRightBack = new Vector3(1.0f, 1.0f, 1.0f) * size;

			// Calculate the position of the vertices on the bottom face.
			_btmLeftFront = new Vector3(0f, 0f, 0f) * size;
			_btmLeftBack = new Vector3(0f, 0f, 1.0f) * size;
			_btmRightFront = new Vector3(1.0f, 0f, 0f) * size;
			_btmRightBack = new Vector3(1.0f, 0f, 1.0f) * size;

			Front = GetFrontVertex(AdjustColor(color, BlockFace.North));
			Back = GetBackVertex(AdjustColor(color, BlockFace.South));
			Left = GetLeftVertex(AdjustColor(color, BlockFace.West));
			Right = GetRightVertex(AdjustColor(color, BlockFace.East));
			Top = GetTopVertex(AdjustColor(color, BlockFace.Up));
			Bottom = GetBottomVertex(AdjustColor(color, BlockFace.Down));
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

		private VertexPositionColor[] GetLeftVertex(Color color)
		{
			// Add the vertices for the RIGHT face. 
			return (new VertexPositionColor[]
			{
				new VertexPositionColor(_topLeftFront, color), new VertexPositionColor(_btmLeftFront, color),
				new VertexPositionColor(_btmLeftBack, color), new VertexPositionColor(_topLeftBack, color),
				new VertexPositionColor(_topLeftFront, color), new VertexPositionColor(_btmLeftBack, color),
			});
		}

		private VertexPositionColor[] GetRightVertex(Color color)
		{
			// Add the vertices for the RIGHT face. 
			return new VertexPositionColor[]
			{
				new VertexPositionColor(_topRightFront, color), new VertexPositionColor(_btmRightBack, color),
				new VertexPositionColor(_btmRightFront, color), new VertexPositionColor(_topRightBack, color),
				new VertexPositionColor(_btmRightBack, color), new VertexPositionColor(_topRightFront, color),
			};
		}

		private VertexPositionColor[] GetFrontVertex(Color color)
		{
			// Add the vertices for the RIGHT face. 
			return new VertexPositionColor[]
			{
				new VertexPositionColor(_topLeftFront, color), new VertexPositionColor(_topRightFront, color),
				new VertexPositionColor(_btmLeftFront, color), new VertexPositionColor(_btmLeftFront, color),
				new VertexPositionColor(_topRightFront, color), new VertexPositionColor(_btmRightFront, color),
			};
		}

		private VertexPositionColor[] GetBackVertex(Color color)
		{
			// Add the vertices for the RIGHT face. 
			return new VertexPositionColor[]
			{
				new VertexPositionColor(_topLeftBack, color), new VertexPositionColor(_btmLeftBack, color),
				new VertexPositionColor(_topRightBack, color), new VertexPositionColor(_btmLeftBack, color),
				new VertexPositionColor(_btmRightBack, color), new VertexPositionColor(_topRightBack, color),
			};
		}

		private VertexPositionColor[] GetTopVertex(Color color)
		{
			// Add the vertices for the RIGHT face. 
			return new VertexPositionColor[]
			{
				new VertexPositionColor(_topLeftFront, color), new VertexPositionColor(_topLeftBack, color),
				new VertexPositionColor(_topRightBack, color), new VertexPositionColor(_topLeftFront, color),
				new VertexPositionColor(_topRightBack, color), new VertexPositionColor(_topRightFront, color),
			};
		}

		private VertexPositionColor[] GetBottomVertex(Color color)
		{
			// Add the vertices for the RIGHT face. 
			return new VertexPositionColor[]
			{
				new VertexPositionColor(_btmLeftFront, color), new VertexPositionColor(_btmRightBack, color),
				new VertexPositionColor(_btmLeftBack, color), new VertexPositionColor(_btmLeftFront, color),
				new VertexPositionColor(_btmRightFront, color), new VertexPositionColor(_btmRightBack, color),
			};
		}
	}
}