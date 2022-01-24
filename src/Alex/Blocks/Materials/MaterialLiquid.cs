using Alex.Utils;

namespace Alex.Blocks.Materials
{
	public class MaterialLiquid : Material
	{
		public MaterialLiquid(MapColor color) : base(color)
		{
			this.SetReplaceable();

			base.IsSolid = false;
			base.BlocksMovement = false;
			base.IsLiquid = true;
		}
	}
}