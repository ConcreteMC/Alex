using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Buttons
{
	public class PolishedBlackStoneButton : Button
	{
		/// <inheritdoc />
		public PolishedBlackStoneButton() : base(5222)
		{
			BlockMaterial = Material.Stone;
		}
	}
}