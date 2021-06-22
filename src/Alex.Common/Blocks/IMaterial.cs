using Alex.Common.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Common.Blocks
{
	public interface IMaterial
	{
		string SoundCategory { get; }
		IMaterial SetSoundCategory(string soundCategory);
		
		IMapColor MapColorValue { get; }
		
		TintType TintType { get; }
		Color TintColor { get; }
		IMaterial SetTintType(TintType type, Color color);
		
		double Slipperiness { get; }

		IMaterial SetSlipperines(double value);
		
		float Hardness { get; }
		IMaterial SetHardness(float hardness);
		
		bool BlocksLight { get; }
		bool BlocksMovement { get; }
		IMaterial SetTranslucent();
		IMaterial SetRequiresTool();
		IMaterial SetBurning();
		bool CanBurn { get; }
		bool IsLiquid { get; }
		bool IsOpaque { get; }
		bool IsReplaceable { get; }
		bool IsSolid { get; }
		bool IsToolRequired { get; }
		IMaterial SetReplaceable();

		bool IsWatterLoggable { get; }

		IMaterial SetWaterLoggable();

		bool CanUseTool(ItemType type, ItemMaterial material);
		IMaterial SetRequiredTool(ItemType type, ItemMaterial material);
		
		IMaterial Clone();
	}

	public interface IMapColor
	{
		int GetMapColor(int index);
	}
}
