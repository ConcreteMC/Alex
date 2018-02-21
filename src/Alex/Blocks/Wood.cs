using Alex.Utils;

namespace Alex.Blocks
{
	public class Wood : Block
	{
		public Wood(byte metadata) : base(17, metadata)
		{
			switch (metadata)
			{
				case 0:
					SetTexture(TextureSide.Side, "log_oak");
					SetTexture(TextureSide.Top, "log_oak_top");
					SetTexture(TextureSide.Bottom, "log_oak_top");
					break;
				case 1:
					SetTexture(TextureSide.Side, "log_spruce");
					SetTexture(TextureSide.Top, "log_spruce_top");
					SetTexture(TextureSide.Bottom, "log_spruce_top");
					break;
				case 2:
					SetTexture(TextureSide.Side, "log_birch");
					SetTexture(TextureSide.Top, "log_birch_top");
					SetTexture(TextureSide.Bottom, "log_birch_top");
					break;
				case 3:
					SetTexture(TextureSide.Side, "log_jungle");
					SetTexture(TextureSide.Top, "log_jungle_top");
					SetTexture(TextureSide.Bottom, "log_jungle_top");
					break;
				default:
					SetTexture(TextureSide.Side, "log_jungle");
					SetTexture(TextureSide.Top, "log_jungle_top");
					SetTexture(TextureSide.Bottom, "log_jungle_top");
					break;
			}
		}
	}
}
