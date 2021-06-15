using Alex.Common.Gui.Graphics;
using Alex.Entities.Components.Effects;
using RocketUI;

namespace Alex.Utils
{
	public static class EffectExtensions
	{
		public static bool TryGetTexture(this EffectType effectType, out GuiTextures? texture)
		{
			texture = null;
			return false;
		}
	}
}