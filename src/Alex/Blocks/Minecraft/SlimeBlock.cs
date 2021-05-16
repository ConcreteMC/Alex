namespace Alex.Blocks.Minecraft
{
	public class SlimeBlock : Block
	{
		public SlimeBlock() : base()
		{
			Solid = true;
			Transparent = true;
			IsFullCube = true;

			BlockMaterial = Material.Slime;
		}
	}
}
