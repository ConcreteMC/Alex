namespace Alex.Blocks.Minecraft
{
	public class StonePressurePlate : PressurePlate
	{
		
	}

	public class PressurePlate : Block
	{
		public PressurePlate() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			
			Hardness = 0.5f;
		}
	}
}
