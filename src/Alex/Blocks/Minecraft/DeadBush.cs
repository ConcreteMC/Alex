namespace Alex.Blocks.Minecraft
{
	public class DeadBush : Block
	{
		public DeadBush() : base(953)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Plants;
		}
	}
}
