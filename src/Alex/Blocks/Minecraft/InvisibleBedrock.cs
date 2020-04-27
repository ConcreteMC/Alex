namespace Alex.Blocks.Minecraft
{
	public class InvisibleBedrock : Block
	{
		public InvisibleBedrock(bool pe = true) : base(pe ? 95 : 166, 0)
		{
			Renderable = false;
			HasHitbox = false;
			Solid = true;
			
			Hardness = 60000;
		}
	}
}
