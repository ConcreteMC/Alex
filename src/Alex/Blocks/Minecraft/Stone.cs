namespace Alex.Blocks.Minecraft
{
	public class Stone : Block
	{
		public Stone() : base(1)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
