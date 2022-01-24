using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Doors
{
	public class AcaciaDoor : Door
	{
		public AcaciaDoor() : base(7790)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Wood;
		}
	}
}