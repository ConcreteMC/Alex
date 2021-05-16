namespace Alex.Blocks.Minecraft
{
	public class CraftingTable : Block
	{
		public CraftingTable() : base()
		{
			Solid = true;
			Transparent = false;

			CanInteract = true;
			
			BlockMaterial = Material.Wood.SetHardness(2.5f);
		}
	}
}
