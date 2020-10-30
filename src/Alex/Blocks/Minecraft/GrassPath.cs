namespace Alex.Blocks.Minecraft
{
	public class GrassPath : Block
	{
		public GrassPath() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 0.6f;
			BlockMaterial = Material.Grass;
		}
	}
}
