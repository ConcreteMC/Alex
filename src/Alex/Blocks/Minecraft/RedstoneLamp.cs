using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class RedstoneLamp : Block
	{
		public RedstoneLamp() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.RedstoneLight;
		}

		public override byte Luminance
		{
			get
			{
				if (Lit.GetValue(BlockState))
				{
					return 15;
				}

				return 0;
			}
			set
			{
				
			}
		}
	}
}
