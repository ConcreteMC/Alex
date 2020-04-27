namespace Alex.Blocks.Minecraft
{
	public class Pumpkin : Block
	{
		public Pumpkin() : base(3402)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 1f;
		}
	}
}
