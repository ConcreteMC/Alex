namespace Alex.Blocks.Minecraft
{
	public class MagmaBlock : Block
	{
		public MagmaBlock() : base(8102)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
			LightValue = 3;
		}
	}
}
