namespace Alex.Blocks.Minecraft
{
	public class RedstoneTorch : Block
	{
		public bool IsWallTorch { get; }
		public RedstoneTorch(bool wallTorch = false) : base(3291)
		{
			IsWallTorch = wallTorch;
			
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 7;
			
			BlockMaterial = Material.RedstoneLight;
		}
	}
}
