using Alex.API.Utils;

namespace Alex.API.Blocks
{
	public interface IMaterial
	{
		double Slipperiness { get; }

		IMaterial SetSlipperines(double value);
		
		float Hardness { get; }
		IMaterial SetHardness(float hardness);
		
		bool BlocksLight();
		bool BlocksMovement();
		IMaterial SetTranslucent();
		IMaterial SetRequiresTool();
		IMaterial SetBurning();
		IMaterial SetAdventureModeExempt();
		IMaterial SetImmovableMobility();
		IMaterial SetNoPushMobility();
		bool GetCanBurn();
		IMapColor GetMaterialMapColor();
		bool IsLiquid();
		bool IsOpaque();
		bool IsReplaceable();
		bool IsSolid();
		bool IsToolRequired();
		IMaterial SetReplaceable();

		bool CanUseTool(ItemType type, ItemMaterial material);
		IMaterial SetRequiredTool(ItemType type, ItemMaterial material);
		
		IMaterial Clone();
	}

	public interface IMapColor
	{
		int GetMapColor(int index);
	}
}
