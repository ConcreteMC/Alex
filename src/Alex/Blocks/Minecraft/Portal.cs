namespace Alex.Blocks.Minecraft
{
	public class Portal : Block
	{
		public Portal() : base()
		{
			Solid = false;
			Transparent = true;

			LightValue = 11;

			BlockMaterial = Material.Portal;
		}
	}
}
