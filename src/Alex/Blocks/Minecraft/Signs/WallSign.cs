using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Signs
{
	public class WallSign : Block
	{
		public WoodType WoodType { get; }

		public WallSign(WoodType woodType = WoodType.Oak) : base()
		{
			WoodType = woodType;

			Solid = false;
			Transparent = true;
			Renderable = false;
			CanInteract = true;

			HasHitbox = true;

			//RequiresUpdate = true

			base.BlockMaterial = Material.Wood.Clone().WithMapColor(woodType.ToMapColor());
		}
	}
}