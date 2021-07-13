using Alex.Utils;

namespace Alex.Blocks.Materials
{
	public class MaterialLogic : Material
	{
		public MaterialLogic(MapColor color) : base(color)
		{
			base.IsSolid = false;
			base.BlocksLight = false;
			base.BlocksMovement = false;
		}
	}
}