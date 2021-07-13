namespace Alex.Blocks.Minecraft
{
	public class StonePressurePlate : PressurePlate
	{
		public StonePressurePlate()
		{
			
		}
	}

	public class PressurePlate : Block
	{
		public PressurePlate() : base()
		{
			Solid = true;
			Transparent = true;
		}
	}
}
