namespace Alex.Blocks.Minecraft
{
	public class Bookshelf : Block
	{
		public Bookshelf() : base(1037)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 1.5f;
		}
	}
}
