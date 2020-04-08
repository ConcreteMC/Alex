namespace Alex.Blocks.Minecraft
{
	public class ChainCommandBlock : Block
	{
		public ChainCommandBlock() : base(8092)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
