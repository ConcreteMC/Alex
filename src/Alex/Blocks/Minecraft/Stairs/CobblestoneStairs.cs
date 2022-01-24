namespace Alex.Blocks.Minecraft.Stairs
{
	public class CobblestoneStairs : Stairs
	{
		public CobblestoneStairs() : base(3110)
		{
			Solid = true;
			Transparent = true;
			IsFullCube = false;
		}
	}
}