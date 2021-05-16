namespace Alex.Blocks.Minecraft
{
	public class GrassPath : Block
	{
		public GrassPath() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Grass;
		}
	}
}
