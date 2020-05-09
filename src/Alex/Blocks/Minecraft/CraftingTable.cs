namespace Alex.Blocks.Minecraft
{
	public class CraftingTable : Block
	{
		public CraftingTable() : base(2960)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 2.5f;

			CanInteract = true;
		}
	}
}
