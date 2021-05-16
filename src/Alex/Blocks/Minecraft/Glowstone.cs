namespace Alex.Blocks.Minecraft
{
	public class Glowstone : Block
	{
		public Glowstone() : base()
		{
			Solid = true;
			Transparent = false;
			LightValue = 15;

			BlockMaterial = Material.Glass;
		}
	}
}
