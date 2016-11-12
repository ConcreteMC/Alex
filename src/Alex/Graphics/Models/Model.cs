using System;
using Alex.Graphics.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
    public class Model
    {
        public Vector3 Size { get; protected set; }
		public Vector3 Offset { get; protected set; }

        public Model()
        {
            Size = Vector3.One;
	        Offset = Vector3.Zero;
        }

        public virtual VertexPositionNormalTextureColor[] GetShape(Vector3 position, Block baseBlock)
        {
            return new VertexPositionNormalTextureColor[0];
        }

        public void SetSize(Vector3 size)
        {
            Size = size;
        }

	    public void SetOffset(Vector3 offset)
	    {
		    Offset = offset;
	    }
    }
}
