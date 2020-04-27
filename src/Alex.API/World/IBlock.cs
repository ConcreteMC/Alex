using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Items;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using MiNET;
using BlockFace = Alex.API.Blocks.BlockFace;

namespace Alex.API.World
{
	public interface IBlock
	{
		bool Solid { get; set; }
		bool Transparent { get; set; }
		bool Animated { get; set; }
		bool Renderable { get; set; }
		bool HasHitbox { get; set; }
		float Drag { get; set; }
		string Name { get; set; }
		string DisplayName { get; set; }
		bool IsFullCube { get; set; }
		bool IsFullBlock { get; set; }
		bool RandomTicked { get; set; }
		bool IsReplacible { get; set; }
		bool RequiresUpdate { get; set; }
		bool CanInteract { get; set; }
		//IBlockState BlockState { get; set; }
		
		int LightValue { get; set; } 
		int LightOpacity { get; set; }
		IBlockState BlockState { get; set; }
		bool IsWater { get; set; }
		IMaterial BlockMaterial { get; set; }
		float Hardness { get; set; }
		
        bool Tick(IWorld world, Vector3 position);
		void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock);
		IBlockState BlockPlaced(IWorld world, IBlockState state, BlockCoordinates position);
		double GetBreakTime(IItem miningTool);

		bool ShouldRenderFace(BlockFace face, IBlock neighbor);

	}
}
