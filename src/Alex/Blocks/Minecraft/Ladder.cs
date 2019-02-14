namespace Alex.Blocks.Minecraft
{
	public class Ladder : Block
	{
		public Ladder() : base(3082)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
