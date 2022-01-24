using Alex.Blocks.Minecraft.Terracotta;

namespace Alex.Blocks.Materials
{
	public class MaterialStainedClay : Material
	{
		/// <inheritdoc />
		public MaterialStainedClay(ClayColor color) : base(color.ToMapColor()) { }
	}
}