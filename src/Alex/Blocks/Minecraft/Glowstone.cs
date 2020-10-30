namespace Alex.Blocks.Minecraft
{
	public class Glowstone : Block
	{
		public Glowstone() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 15;

			Hardness = 0.3f;
			
			BlockMaterial = Material.Glass;
		}
	}
}
