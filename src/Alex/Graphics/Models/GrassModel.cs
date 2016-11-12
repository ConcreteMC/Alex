using Alex.Graphics.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
	public class GrassModel : Model
	{
		public override VertexPositionNormalTextureColor[] GetShape(Vector3 position, Block baseBlock)
		{
			var vertices = new Cube().GetShape(position, baseBlock);
			//TODO: Rotate vertices.
			return vertices;
		}
	}
}
