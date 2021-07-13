using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class RedstoneTorch : Block
	{
		public bool IsWallTorch { get; }
		public RedstoneTorch(bool wallTorch = false) : base()
		{
			IsWallTorch = wallTorch;
			
			Solid = false;
			Transparent = true;
			Luminance = 7;
			
			BlockMaterial = Material.RedstoneLight;
		}
	}
}
