namespace Alex.Blocks.Minecraft
{
	public class EndPortal : Block
	{
		public EndPortal() : base()
		{
			Solid = false;
			Transparent = true;
			LightValue = 15;

			BlockMaterial = Material.Portal;
		}
	}
}
