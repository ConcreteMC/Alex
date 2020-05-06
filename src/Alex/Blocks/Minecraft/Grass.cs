namespace Alex.Blocks.Minecraft
{
	public class Grass : Block
	{
		public Grass() : base(951)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			IsFullBlock = false;
			
			BlockMaterial = Material.Plants;
			LightOpacity = 1;
		}
	}
}
