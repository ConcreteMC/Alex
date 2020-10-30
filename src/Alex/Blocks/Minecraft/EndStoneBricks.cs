namespace Alex.Blocks.Minecraft
{
	public class EndStoneBricks : Block
	{
		public EndStoneBricks() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;

			Hardness = 0.8f;
		}
	}
}
