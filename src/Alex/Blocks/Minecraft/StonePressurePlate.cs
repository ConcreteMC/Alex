namespace Alex.Blocks.Minecraft
{
	public class StonePressurePlate : Block
	{
		public StonePressurePlate() : base(3212)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 0.5f;
		}
	}
}
