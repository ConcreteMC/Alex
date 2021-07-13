using Alex.Common.Items;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Common.Blocks
{
	public interface IMaterial
	{
		string SoundCategory { get; }
		IMaterial WithSoundCategory(string soundCategory);
		
		IMapColor MapColor { get; }

		IMaterial WithMapColor(IMapColor color);
		
		TintType TintType { get; }
		Color TintColor { get; }
		IMaterial WithTintType(TintType type, Color color);
		
		double Slipperiness { get; }

		IMaterial WithSlipperiness(double value);
		
		float Hardness { get; }
		IMaterial WithHardness(float hardness);
		
		bool BlocksLight { get; }
		bool BlocksMovement { get; }
		IMaterial SetTranslucent();
		IMaterial SetRequiresTool();
		IMaterial SetFlammable();
		bool IsFlammable { get; }
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
		Color BaseColor { get; }
		Color GetMapColor(int index);
	}
}
