namespace Alex.Blocks.Minecraft
{
	public class PackedIce : Block
	{
		public PackedIce() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.PackedIce;

			LightOpacity = 4;
		}
	}
}
