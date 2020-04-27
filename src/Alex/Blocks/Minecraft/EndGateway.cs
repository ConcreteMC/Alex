namespace Alex.Blocks.Minecraft
{
	public class EndGateway : Block
	{
		public EndGateway() : base(8073)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 15;
			
			Hardness = -1;
		}
	}
}
