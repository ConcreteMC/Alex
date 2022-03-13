using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Signs;

public class StandingSign : Block
{
	public WoodType WoodType { get; }

	public StandingSign(WoodType woodType = WoodType.Oak)
	{
		WoodType = woodType;

		Solid = false;
		Transparent = true;
		Renderable = false;
		CanInteract = true;

		HasHitbox = true;

		//RequiresUpdate = true;

		BlockMaterial = Material.Wood.Clone().WithMapColor(woodType.ToMapColor());
	}
}