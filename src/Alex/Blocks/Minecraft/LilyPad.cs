namespace Alex.Blocks.Minecraft
{
	public class LilyPad : Block
	{
		public LilyPad() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;

			BlockMaterial = Material.Plants.Clone().SetTranslucent();
		}
	}
}
