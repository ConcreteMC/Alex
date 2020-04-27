namespace Alex.Blocks.Minecraft
{
	public class Furnace : Block
	{
		public Furnace() : base(2978)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 3.5f;
		}
	}
}
