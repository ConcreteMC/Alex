using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Terracotta
{
	public class Terracotta : Block
	{
		public Terracotta(ClayColor color = ClayColor.White)
		{
			Solid = true;
			Transparent = false;

			base.BlockMaterial = new MaterialHardenedStainedClay(color); // Material.Clay;
		}
	}

	public enum ClayColor
	{
		White,
		Orange,
		Magenta,
		LightBlue,
		Yellow,
		Lime,
		Pink,
		Gray,
		Silver,
		Cyan,
		Purple,
		Blue,
		Brown,
		Green,
		Red,
		Black
	}
}