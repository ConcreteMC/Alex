namespace Alex.Blocks.Minecraft
{
	public class GlassPane : Block
	{
		public GlassPane() : base(4152)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullCube = false;

			IsFullCube = false;
			
			BlockMaterial = Material.Glass;
		}
	}
}
