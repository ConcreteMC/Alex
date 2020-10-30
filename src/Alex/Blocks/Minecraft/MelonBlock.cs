namespace Alex.Blocks.Minecraft
{
	public class MelonBlock : Block
	{
		public MelonBlock() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Plants;
			Hardness = 1f;
		}
	}
}
