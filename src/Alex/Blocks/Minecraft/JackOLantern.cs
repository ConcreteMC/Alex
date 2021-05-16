namespace Alex.Blocks.Minecraft
{
	public class JackOLantern : Block
	{
		public JackOLantern()
		{
			Solid = true;
			Transparent = false;

			LightValue = 15;
			
			BlockMaterial = Material.Wood.Clone().SetHardness(1);
		}
	}
}