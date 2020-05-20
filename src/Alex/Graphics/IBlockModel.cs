using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
	public interface IBlockModel
	{
		BoundingBox BoundingBox { get; }
		(VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IBlockAccess world, Vector3 position, Block baseBlock);
		BoundingBox GetBoundingBox(Vector3 position, Block requestingBlock);
		BoundingBox? GetPartBoundingBox(Vector3 position, BoundingBox entityBox);
		BoundingBox[] GetIntersecting(Vector3 position, BoundingBox box);
	}
}
