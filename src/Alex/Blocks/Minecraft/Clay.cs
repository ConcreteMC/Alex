namespace Alex.Blocks.Minecraft
{
	public class Clay : Block
	{
		public Clay() : base(3351)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Clay;
		}
	}
}
