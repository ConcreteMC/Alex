namespace Alex.Blocks.Minecraft
{
	public class LapisBlock : Block
	{
		public LapisBlock() : base(142)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 3f;
		}
	}
}
