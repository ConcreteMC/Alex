using Alex.Blocks.Materials;
using Alex.Blocks.Properties;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Interfaces;

namespace Alex.Blocks.Minecraft.Fences
{
	public class FenceGate : OpenableBlockBase
	{
		public static PropertyBool IN_WALL => new PropertyBool("in_wall");

		public FenceGate() : this(0) { }

		public FenceGate(uint id) : base()
		{
			Solid = true;
			Transparent = true;

			CanInteract = true;
			IsFullCube = false;

			BlockMaterial = Material.Wood;
		}

		/// <inheritdoc />
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block is Fence || block is FenceGate)
				return true;

			return base.CanAttach(face, block);
		}

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			if (prop == "in_wall")
			{
				stateProperty = IN_WALL;

				return true;
			}

			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}