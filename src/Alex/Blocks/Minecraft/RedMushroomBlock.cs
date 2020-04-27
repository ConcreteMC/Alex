namespace Alex.Blocks.Minecraft
{
	public class RedMushroomBlock : Block
	{
		public RedMushroomBlock() : base(3961)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.2f;
		}
	}
}
