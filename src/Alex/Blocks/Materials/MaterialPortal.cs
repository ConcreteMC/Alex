using Alex.Utils;

namespace Alex.Blocks.Materials
{
	public class MaterialPortal : Material
	{
		public MaterialPortal(MapColor color) : base(color)
		{
			base.IsSolid = false;
			base.BlocksLight = false;
			base.BlocksMovement = false;
		}
	}
}