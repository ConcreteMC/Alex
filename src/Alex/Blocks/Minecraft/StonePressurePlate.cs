namespace Alex.Blocks.Minecraft
{
	public class StonePressurePlate : Block
	{
		public StonePressurePlate() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 0.5f;
		}
	}
}
