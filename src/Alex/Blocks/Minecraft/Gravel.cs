namespace Alex.Blocks.Minecraft
{
	public class Gravel : Block
	{
		public Gravel() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.6f;
		}
	}
}
