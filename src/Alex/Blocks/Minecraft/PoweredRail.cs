namespace Alex.Blocks.Minecraft
{
	public class PoweredRail : Block
	{
		public PoweredRail() : base(920)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
