namespace Alex.Blocks.Minecraft
{
	public class EndPortal : Block
	{
		public EndPortal() : base(4535)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 15;
			
			Hardness = -1;
		}
	}
}
