namespace Alex.Blocks.Minecraft
{
	public class EndGateway : Block
	{
		public EndGateway() : base()
		{
			Solid = true;
			Transparent = false;
			LightValue = 15;

			BlockMaterial = Material.Portal;
		}
	}
}
