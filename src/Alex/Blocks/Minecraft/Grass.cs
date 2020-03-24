namespace Alex.Blocks.Minecraft
{
	public class Grass : Block
	{
		public Grass() : base(951)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			IsBlockNormalCube = false;
			IsFullBlock = false;
			
			BlockMaterial = Material.Grass;
			LightOpacity = 0;
		}
	}
}
