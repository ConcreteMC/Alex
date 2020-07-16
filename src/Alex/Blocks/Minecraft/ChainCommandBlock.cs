namespace Alex.Blocks.Minecraft
{
	public class ChainCommandBlock : CommandBlock
	{
		public ChainCommandBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
		}
	}
}
