namespace Alex.Blocks.Minecraft
{
	public class QuartzBlock : Block
	{
		public QuartzBlock() : base(5605)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.8f;
		}
	}
}
