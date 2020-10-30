namespace Alex.Blocks.Minecraft
{
	public class Snow : Block
	{
		public Snow() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullCube = false;
			//BlockMaterial = Material.Snow;
			
			Hardness = 0.2f;
		}
	}
}
