namespace Alex.Blocks.Minecraft
{
	public class StickyPiston : Block
	{
		public StickyPiston() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Piston;
			Hardness = 0.5f;
		}
	}
}
