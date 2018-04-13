using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API
{
	public interface IFontRenderer
	{
		int DrawString(SpriteBatch sb, string text, float x, float y, int color, bool dropShadow, Vector2 scale);
		int DrawString(SpriteBatch sb, string text, float x, float y, int color, bool dropShadow);
		int DrawString(SpriteBatch sb, string text, int x, int y, int color);
		int GetCharWidth(char character);
		int GetStringWidth(string text);
		Vector2 GetStringSize(string text, Vector2 scale);
	}
}
