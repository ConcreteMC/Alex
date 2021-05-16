namespace Alex.Blocks.Minecraft
{
	public class SeaLantern : Block
	{
		public SeaLantern() : base()
		{
			Solid = true;
			Transparent = false;
			LightValue = 15;

			BlockMaterial = Material.Glass;
		}
	}
}
