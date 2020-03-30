namespace Alex.Blocks.Minecraft
{
	public class Bricks : Block
	{
		public Bricks() : base(1035)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
