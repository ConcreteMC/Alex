namespace Alex.Blocks.Minecraft
{
	public class Clay : Block
	{
		public Clay() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Clay;
		}
	}
}
