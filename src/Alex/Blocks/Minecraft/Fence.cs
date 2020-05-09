using Alex.API.Blocks;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class Fence : Block
	{
		public Fence(uint blockStateId) : base()
		{
			Transparent = true;
			Solid = true;
			IsFullCube = false;
		}

		public Fence(string name) : base()
		{
			Transparent = true;
			Solid = true;
			IsFullCube = false;
		}

		public Fence()
		{
			Transparent = true;
			Solid = true;
			IsFullCube = false;
		}

		/// <inheritdoc />
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block is Fence || block is FenceGate)
				return true;
			
			return base.CanAttach(face, block);
		}
	}
}