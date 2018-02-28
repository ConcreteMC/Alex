namespace Alex.CoreRT.Blocks
{
	public class InvisibleBedrock : Block
	{
		public InvisibleBedrock(bool pe = true) : base(pe ? 95 : 166, 0)
		{
			Renderable = false;
			HasHitbox = false;
		}
	}
}
