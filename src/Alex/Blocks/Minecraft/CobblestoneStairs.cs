namespace Alex.Blocks.Minecraft
{
	public class CobblestoneStairs : Stairs
	{
		public CobblestoneStairs() : base(3110)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
			LightOpacity = 16;
		}
	}
}
