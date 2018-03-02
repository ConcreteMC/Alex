using Alex.API.Blocks.State;
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
		//IBlockState BlockState { get; set; }

		bool IsSideSolid(string side);

		bool BlockUpdate(IWorld world, Vector3 position);
	}
}
