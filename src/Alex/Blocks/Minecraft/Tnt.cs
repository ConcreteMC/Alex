namespace Alex.Blocks.Minecraft
{
	public class Tnt : Block
	{
		public Tnt() : base(1036)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Tnt;
		}
	}
}
