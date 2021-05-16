namespace Alex.Blocks.Minecraft
{
	public class LilyPad : Block
	{
		public LilyPad() : base()
		{
			Solid = true;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Plants.Clone().SetTranslucent();
		}
	}
}
