namespace Alex.Blocks.Minecraft
{
	public class GrassBlock : Block
	{
		public GrassBlock() : base(9)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 16;


		}
	}
}
