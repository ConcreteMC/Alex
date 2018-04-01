using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface IBlockModel
	{
		VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, IBlock baseBlock);
		BoundingBox GetBoundingBox(Vector3 position, IBlock requestingBlock);
	}
}
