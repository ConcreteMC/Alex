using Alex.CoreRT.Gamestates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.CoreRT.Rendering.UI
{
    public class Image : UIComponent
    {
        public Texture2D Texture { get; set; }
        public Image(Texture2D texture)
        {
            Texture = texture;
            Size = new Vector2(texture.Width, texture.Height);
        }

        public override void Render(RenderArgs args)
        {
            args.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            args.SpriteBatch.Draw(Texture, Location, Color.White);

            args.SpriteBatch.End();
        }
    }
}
