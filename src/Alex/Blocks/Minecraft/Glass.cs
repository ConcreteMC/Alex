namespace Alex.Blocks.Minecraft
{
	public class Glass : Block
	{
		public Glass() : base(140)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
