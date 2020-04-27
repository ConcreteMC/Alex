namespace Alex.Blocks.Minecraft
{
	public class EndPortalFrame : Block
	{
		public EndPortalFrame() : base(4540)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 1;
			
			Hardness = 60000;
		}
	}
}
