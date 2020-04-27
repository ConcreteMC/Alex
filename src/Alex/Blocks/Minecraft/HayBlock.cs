namespace Alex.Blocks.Minecraft
{
	public class HayBlock : Block
	{
		public HayBlock() : base(6731)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.5f;
		}
	}
}
