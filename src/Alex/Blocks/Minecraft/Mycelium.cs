namespace Alex.Blocks.Minecraft
{
	public class Mycelium : Block
	{
		public Mycelium() : base(4403)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 0.6f;
		}
	}
}
