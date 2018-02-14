using Alex.Graphics.Items;
using Alex.Utils;

namespace Alex.Blocks
{
	public class HardenenedClay : Block
	{
		public HardenenedClay() : base(172, 0)
		{
			SetTexture(TextureSide.All, "hardened_clay");

			Solid = true;
			Transparent = false;
		}
	}
}
