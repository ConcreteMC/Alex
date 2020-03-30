namespace Alex.Blocks.Minecraft
{
	public class CommandBlock : Block
	{
		public CommandBlock() : base(5040)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
