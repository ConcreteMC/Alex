namespace Alex.Blocks.Minecraft
{
	public class StickyPiston : Block
	{
		public StickyPiston() : base(944)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Piston;
			Hardness = 0.5f;
		}
	}
}
