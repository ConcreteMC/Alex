namespace Alex.Blocks.Minecraft
{
	public class EndRod : Block
	{
		public EndRod() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 14;

			BlockMaterial = Material.Glass;
			IsFullCube = false;
		}
	}
}
