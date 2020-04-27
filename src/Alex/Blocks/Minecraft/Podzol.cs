namespace Alex.Blocks.Minecraft
{
	public class Podzol : Block
	{
		public Podzol() : base(13)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 0.5f;
		}
	}
}
