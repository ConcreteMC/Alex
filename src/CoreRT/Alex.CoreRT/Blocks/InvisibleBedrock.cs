namespace Alex.CoreRT.Blocks
{
	public class InvisibleBedrock : Block
	{
		public InvisibleBedrock() : base(95, 0)
		{
			Renderable = false;
			HasHitbox = false;
		}
	}
}
