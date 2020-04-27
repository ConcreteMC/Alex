namespace Alex.Blocks.Minecraft
{
	public class Gravel : Block
	{
		public Gravel() : base(68)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.6f;
		}
	}
}
