namespace Alex.Blocks.Minecraft
{
	public class MagmaBlock : Block
	{
		public MagmaBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;
			
			LightValue = 3;
		}
	}
}
