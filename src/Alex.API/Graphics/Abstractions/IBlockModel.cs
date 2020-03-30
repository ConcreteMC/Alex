using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface IBlockModel
	{
		BoundingBox BoundingBox { get; }
		(VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IWorld world, Vector3 position, IBlock baseBlock);
		BoundingBox GetBoundingBox(Vector3 position, IBlock requestingBlock);
		BoundingBox? GetPartBoundingBox(Vector3 position, BoundingBox entityBox);
		BoundingBox[] GetIntersecting(Vector3 position, BoundingBox box);
	}
}
