using Alex.Gamestates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace Alex.Rendering.UI
{
    public class Button : UIComponent
    {
        public delegate void OnClickedDelegate();

        public event OnClickedDelegate OnButtonClick;

        private Rectangle ButtonRectangle { get; set; }
        private Texture2D ButtonTexture { get; set; }
        private Texture2D HoverTexture { get; set; }
        private bool Hovering { get; set; }
        public string Text { get; set; }
        public Button(string text)
        {
            Text = text;
            Hovering = false;
            Size = new Vector2(400, 40);
            HoverTexture = ResManager.ImageToTexture2D(Properties.Resources.ButtonState2);
            ButtonTexture = ResManager.ImageToTexture2D(Properties.Resources.ButtonState1);
        }

        public override void Render(RenderArgs args)
        {
            args.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

			ButtonRectangle = new Rectangle((int) Location.X,(int) Location.Y, (int) Size.X, (int) Size.Y);
            args.SpriteBatch.Draw(Hovering ? HoverTexture : ButtonTexture, ButtonRectangle, Color.Cornsilk);

            var measureString = Alex.Font.MeasureString(Text);

            args.SpriteBatch.DrawString(Alex.Font, Text,
                new Vector2(Location.X + Size.X / 2 - (measureString.X / 2),
                    Location.Y + Size.Y / 2 - (measureString.Y / 2)), Color.White);

            args.SpriteBatch.End();
        }

        public override void Update(GameTime time)
        {
            var ms = Mouse.GetState();
            var mouseRec = new Rectangle(ms.X, ms.Y, 1, 1);

            if (mouseRec.Intersects(ButtonRectangle))
            {
                Hovering = true;
                if (ms.LeftButton == ButtonState.Pressed && OnButtonClick != null)
                {
                    OnButtonClick.Invoke();
                }
            }
            else
            {
                Hovering = false;
            }
        }
    }
}
