namespace Alex.API.Blocks
{
	public interface IMaterial
	{
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
		bool IsToolNotRequired();
		IMaterial SetReplaceable();
	}

	public interface IMapColor
	{
		int GetMapColor(int index);
	}
}
