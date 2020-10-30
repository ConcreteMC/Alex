namespace Alex.Blocks.Minecraft
{
	public class EndPortal : Block
	{
		public EndPortal() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 15;
			
			Hardness = -1;
			
			BlockMaterial = Material.Portal;
		}
	}
}
