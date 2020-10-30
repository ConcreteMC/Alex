namespace Alex.Blocks.Minecraft
{
	public class Clay : Block
	{
		public Clay() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 0.6f;
			BlockMaterial = Material.Clay;
		}
	}
}
