namespace Alex.Blocks.Minecraft
{
	public class LapisBlock : Block
	{
		public LapisBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 3f;
		}
	}
}
