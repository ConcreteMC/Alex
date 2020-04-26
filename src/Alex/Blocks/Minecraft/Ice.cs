namespace Alex.Blocks.Minecraft
{
	public class Ice : Block
	{
		public Ice() : base(3333)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullCube = true;
			
			LightOpacity = 4;
			
			BlockMaterial = Material.Ice;
		}
	}
}
