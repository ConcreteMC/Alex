namespace Alex.Blocks.Minecraft
{
	public class Cobweb : Block
	{
		public Cobweb() : base()
		{
			Solid = false;
			Transparent = true;
			IsFullCube = false;
			LightOpacity = 2;

			BlockMaterial = Material.Web;
		}
	}
}
