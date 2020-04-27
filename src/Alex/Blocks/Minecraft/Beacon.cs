namespace Alex.Blocks.Minecraft
{
	public class Beacon : Block
	{
		public Beacon() : base(5046)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 15;

			Hardness = 3;
		}
	}
}
