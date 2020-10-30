namespace Alex.Blocks.Minecraft
{
	public class Bookshelf : Block
	{
		public Bookshelf() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 1.5f;
		}
	}
}
