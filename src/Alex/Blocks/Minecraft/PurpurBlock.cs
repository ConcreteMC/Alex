namespace Alex.Blocks.Minecraft
{
	public class PurpurBlock : Block
	{
		public PurpurBlock() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.SetHardness(1.5f);
		}
	}
}
