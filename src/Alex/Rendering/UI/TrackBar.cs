using System;
using Alex.Gamestates;
using Alex.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.Rendering.UI
{
    public class TrackBar : UIComponent
    {
        private Rectangle ButtonRectangle { get; set; }
        private Rectangle TrackerRectangle { get; set; }
        private Texture2D ButtonTexture { get; set; }
        private Texture2D TrackerTexture { get; set; }

        public string Text { get; set; }
        private bool Focus { get; set; }
        public int Value { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public TrackBar()
        {
            Text = "";

            Value = 50;
            MaxValue = 100;
            MinValue = 0;
            Size = new Vector2(400, 40);
            ButtonTexture = ResManager.ImageToTexture2D(Resources.ButtonState0);
            TrackerTexture = ResManager.ImageToTexture2D(Resources.ButtonState1);
            Focus = false;
        }

        public override void Render(RenderArgs args)
        {
            ButtonRectangle = new Rectangle((int)Location.X, (int)Location.Y, (int)Size.X, (int)Size.Y);
            var x = (int) ((int) Size.X - Size.X/(MaxValue)*Value);

            if (x + 13 > (Location.X + Size.X)) x = (int) (Location.X + Size.X) - 13;
            if (x < Location.X) x = (int) (Location.X + 3);

            TrackerRectangle = new Rectangle(x, (int)Location.Y, 10, (int)Size.Y);

            Color color = Color.Gray;

            var s = Text;

            var measureString = Alex.Font.MeasureString(s);
            while (measureString.X > ButtonRectangle.Width - 13)
            {
                s = s.Remove(0, 1);
                measureString = Alex.Font.MeasureString(s);
            }

            args.SpriteBatch.Begin();

            args.SpriteBatch.Draw(ButtonTexture, ButtonRectangle, Color.Cornsilk);
            args.SpriteBatch.DrawString(Alex.Font, s,
                new Vector2(Location.X + Size.X / 2 - measureString.X / 2, Location.Y + measureString.Y / 2 - 3), color);

            args.SpriteBatch.Draw(TrackerTexture, TrackerRectangle, Color.Cornsilk);

            args.SpriteBatch.End();
        }

        public override void Update(GameTime time)
        {
            var ms = Mouse.GetState();
            var mouseRec = new Rectangle(ms.X, ms.Y, 1, 1);

            if (ms.LeftButton == ButtonState.Pressed)
            {
                if (mouseRec.Intersects(ButtonRectangle))
                {
                    var a = (ButtonRectangle.X + ButtonRectangle.Width - ms.X) / 4;
                    if (a > MaxValue) a = MaxValue;
                    if (a < MinValue) a = MinValue;

                    Value = (int) a;
                }
            }
        }
    }
}
