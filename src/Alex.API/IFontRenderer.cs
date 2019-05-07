using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API
{
	public interface IFontRenderer
	{
		int DrawString(SpriteBatch sb, string text, Vector2 position, Color color, bool dropShadow, Vector2 scale, float rotation, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f);
		int DrawString(SpriteBatch sb, string text, Vector2 position, Color color, bool dropShadow, Vector2 scale);
		int DrawString(SpriteBatch sb, string text, Vector2 position, Color color, bool dropShadow);
		int DrawString(SpriteBatch sb, string text, int x, int y, Color color);
		int GetCharWidth(char character);
		int GetStringWidth(string text);
		Vector2 GetStringSize(string text, Vector2 scale);
	}
}
