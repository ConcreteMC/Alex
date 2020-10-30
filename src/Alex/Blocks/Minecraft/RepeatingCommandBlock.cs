namespace Alex.Blocks.Minecraft
{
	public class RepeatingCommandBlock : Block
	{
		public RepeatingCommandBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			Animated = true;
		}
	}
}
