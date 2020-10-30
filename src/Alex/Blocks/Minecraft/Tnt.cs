namespace Alex.Blocks.Minecraft
{
	public class Tnt : Block
	{
		public Tnt() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Tnt;
		}
	}
}
