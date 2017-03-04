using Alex.Graphics.Items;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
	public class GrassModel : Model
	{
		public override VertexPositionNormalTextureColor[] GetShape(World world, Vector3 position, Block baseBlock)
		{
			var vertices = new Cube().GetShape(world,position, baseBlock);
			//TODO: Rotate vertices.
			return vertices;
		}
	}
}
