namespace Alex.Blocks.Minecraft
{
	public class NetherWartBlock : Block
	{
		public NetherWartBlock() : base(8103)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
