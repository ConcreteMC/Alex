using Alex.Utils;

namespace Alex.Blocks
{
	public class Sand : Block
	{
		public Sand(byte meta = 0) : base(12, meta)
		{
			if (meta == 1)
			{
				SetTexture(TextureSide.All, "red_sand");
			}
			else
			{
				SetTexture(TextureSide.All, "sand");
			}

			Solid = true;
			Transparent = false;
		}
	}
}
