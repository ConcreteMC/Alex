namespace Alex.Blocks.Minecraft
{
	public class Cobweb : Block
	{
		public Cobweb() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
			LightOpacity = 2;
			
			Hardness = 4;
			BlockMaterial = Material.Web;
		}
	}
}
