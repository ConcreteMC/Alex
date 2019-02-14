namespace Alex.Blocks.Minecraft
{
	public class SlimeBlock : Block
	{
		public SlimeBlock() : base(6402)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = true;
		}
	}
}
