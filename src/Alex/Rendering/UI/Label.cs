using Alex.Gamestates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering.UI
{
    public class Label : UIComponent
    {
        public SpriteFont Font { get; set; }
        public Color Color { get; set; }
        public string Text { get; set; }
        public Label(string text)
        {
            Text = text;
            Color = Color.White;
            Font = Alex.Font;
        }

        public override void Render(RenderArgs args)
        {
            args.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            args.SpriteBatch.DrawString(Font, Text,
               Location, Color);

            args.SpriteBatch.End();
        }
    }
}
