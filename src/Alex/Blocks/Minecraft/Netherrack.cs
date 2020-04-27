namespace Alex.Blocks.Minecraft
{
	public class Netherrack : Block
	{
		public Netherrack() : base(3403)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.4f;
		}
	}
}
