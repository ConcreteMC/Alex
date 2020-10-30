namespace Alex.Blocks.Minecraft
{
	public class QuartzBlock : Block
	{
		public QuartzBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.8f;
		}
	}
}
