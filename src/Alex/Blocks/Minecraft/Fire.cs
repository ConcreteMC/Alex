namespace Alex.Blocks.Minecraft
{
	public class Fire : Block
	{
		public Fire() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;
			Animated = true;
			
			LightValue = 15;

			BlockMaterial = Material.Fire;
		}
	}
}
