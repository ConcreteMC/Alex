using Microsoft.Xna.Framework;

namespace Alex.Common.Blocks
{
	public enum TintType
	{
		Default,
		Color,
		Grass,
		Foliage,
		Water
	}

	public interface ITinted
	{
		TintType TintType { get; }
		Color TintColor { get; }
	}
}