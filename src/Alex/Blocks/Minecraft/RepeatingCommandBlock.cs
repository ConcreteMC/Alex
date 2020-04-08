namespace Alex.Blocks.Minecraft
{
	public class RepeatingCommandBlock : Block
	{
		public RepeatingCommandBlock() : base(8080)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
