namespace Alex.Blocks.Minecraft
{
	public class BrownMushroomBlock : Block
	{
		public BrownMushroomBlock() : base(3897)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.2f;
		}
	}
}
