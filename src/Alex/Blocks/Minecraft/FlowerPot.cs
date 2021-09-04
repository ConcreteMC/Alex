namespace Alex.Blocks.Minecraft
{
	public class FlowerPot : Block
	{
		public FlowerPot() : base()
		{
			Solid = true;
			Transparent = true;
			base.IsFullCube = false;
		}
	}
}
