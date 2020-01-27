using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface IBlockModel
	{
		(VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 position, IBlock baseBlock);
		BoundingBox GetBoundingBox(Vector3 position, IBlock requestingBlock);
		BoundingBox GetPartBoundingBox(Vector3 position, Vector3 entityPosition);
	}
}
