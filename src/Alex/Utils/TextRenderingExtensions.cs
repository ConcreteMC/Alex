using Alex.API;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
    public static class TextRenderingExtensions
    {
        public static void RenderText(this IRenderArgs renderArgs, Vector3 vector, string text)
        {
            Vector2 textPosition;

            // calculate screenspace of text3d space position
            var screenSpace = renderArgs.GraphicsDevice.Viewport.Project(Vector3.Zero,
                renderArgs.Camera.ProjectionMatrix,
                renderArgs.Camera.ViewMatrix,
                Matrix.CreateTranslation(vector));


            // get 2D position from screenspace vector
            textPosition.X = screenSpace.X;
            textPosition.Y = screenSpace.Y;

            float s = 1f;
            var scale = new Vector2(s, s);
	
            string clean = text;

            var stringCenter = Alex.Font.MeasureString(clean, s);
            var c = new Point((int)stringCenter.X, (int)stringCenter.Y);

            textPosition.X = (int)(textPosition.X - c.X);
            textPosition.Y = (int)(textPosition.Y - c.Y);

            renderArgs.SpriteBatch.FillRectangle(new Rectangle(textPosition.ToPoint(), c), new Color(Color.Black, 128));
            renderArgs.SpriteBatch.DrawString(Alex.Font, clean, textPosition, TextColor.White, FontStyle.None, 0f, Vector2.Zero, scale);
        }
    }
}