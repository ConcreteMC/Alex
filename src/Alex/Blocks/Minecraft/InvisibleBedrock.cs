namespace Alex.Blocks.Minecraft
{
	public class InvisibleBedrock : Block
	{
		public InvisibleBedrock(bool pe = true) : base()
		{
			Renderable = false;
			HasHitbox = false;
			Solid = true;
			Transparent = true;
			
			Hardness = 60000;
		}
	}
}
