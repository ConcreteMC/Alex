using Alex.Utils;

namespace Alex.Blocks.Materials
{
	public class MaterialTransparent : Material
	{
		public MaterialTransparent(MapColor color) : base(color)
		{
			this.SetReplaceable();

			base.IsSolid = false;
			base.BlocksLight = false;
			base.BlocksMovement = false;
		}
	}
}