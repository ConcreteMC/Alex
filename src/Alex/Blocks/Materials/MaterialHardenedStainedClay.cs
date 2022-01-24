using Alex.Blocks.Minecraft.Terracotta;
using Alex.Common.Items;

namespace Alex.Blocks.Materials
{
	public class MaterialHardenedStainedClay : MaterialStainedClay
	{
		/// <inheritdoc />
		public MaterialHardenedStainedClay(ClayColor color) : base(color)
		{
			Hardness = 1.25f;

			SetRequiredTool(ItemType.PickAxe);
		}
	}
}