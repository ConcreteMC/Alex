using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.World
{
	public interface IBlock
	{
		uint BlockStateID { get; }
		int BlockId { get; }
		byte Metadata { get; }
		bool Solid { get; set; }
		bool Transparent { get; set; }
		bool Renderable { get; set; }
		bool HasHitbox { get; set; }
		float Drag { get; set; }
		string DisplayName { get; set; }
		bool IsBlockNormalCube { get; set; }
		bool IsFullCube { get; set; }
		bool IsFullBlock { get; set; }
		bool RandomTicked { get; set; }
		bool IsReplacible { get; set; }
		//IBlockState BlockState { get; set; }

		double AmbientOcclusionLightValue { get; set; }
		int LightValue { get; set; } 
		int LightOpacity { get; set; }
		IBlockState BlockState { get; set; }

		bool IsSideSolid(string side);

		bool Tick(IWorld world, Vector3 position);
		VertexPositionNormalTextureColor[] GetVertices(Vector3 position, IWorld world);
	}
}
