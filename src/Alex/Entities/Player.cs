using Alex.Gamestates;
using Alex.Graphics.Items;
using Alex.Graphics.Models;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Entities
{
    public class Player : Entity
    {
        public static readonly float EyeLevel = 1.625F;

	    public override void Render(Vector3 position)
	    {
		    var cube = new Cube();
			cube.SetSize(new Vector3(1f, 1.8f, 1f));
		    var shape = cube.GetShape(position, new Block(0, 0));
			Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, shape, 0, shape.Length / 3, VertexPositionNormalTextureColor.VertexDeclaration);
	    }
    }
}