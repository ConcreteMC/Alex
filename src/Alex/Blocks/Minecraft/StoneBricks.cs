namespace Alex.Blocks.Minecraft
{
	public class StoneBricks : Block
	{
		public StoneBricks() : base(3893)
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
