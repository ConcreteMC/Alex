namespace Alex.Blocks.Minecraft
{
	public class Bookshelf : Block
	{
		public Bookshelf() : base()
		{
			Solid = true;
			Transparent = false;
			BlockMaterial = Material.Wood.Clone().SetHardness(1.5f);
		}
	}
}
