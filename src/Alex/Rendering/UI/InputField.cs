using System;
using System.Linq;
using System.Runtime.InteropServices;
using Alex.Gamestates;
using Alex.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.Rendering.UI
{
    public class InputField : UIComponent
    {
        private Rectangle ButtonRectangle { get; set; }
        private Texture2D ButtonTexture { get; set; }

        public string PlaceHolder { get; set; }
        public string Text { get; set; }
        private bool Focus { get; set; }
        public bool PasswordField { get; set; }

        public InputField()
        {
            PlaceHolder = "This is a placeholder...";
            Text = "";
            LastUpdate = DateTime.MinValue;
            LastChange = DateTime.MinValue;

            Size = new Vector2(400, 40);
            ButtonTexture = ResManager.ImageToTexture2D(Resources.ButtonState0);
            Focus = false;
            PasswordField = false;

            Alex.Instance.OnCharacterInput += OnCharacterInput;
            PrevMouseState = Mouse.GetState();
        }

        private void OnCharacterInput(object sender, char c)
        {
            if (!Focus) return;
            Text += c;
        }

        private bool DoThing = false;
        public override void Render(RenderArgs args)
        {
            ButtonRectangle = new Rectangle((int)Location.X, (int)Location.Y, (int)Size.X, (int)Size.Y);
            Color color = Color.White;

            var s = Text;
            bool useph = false;
            if (!Focus)
            {
                if (Text == "")
                {
                    s = PlaceHolder;
                    useph = true;
                    color = Color.Gray;
                }
                
                DoThing = false;
            }

            if (!useph && PasswordField)
            {
                var b = s.Length;
                s = "";
                for (int i = 0; i < b; i++)
                {
                    s += '*';
                }
            }
            

            if (DoThing)
            {
                s += '_';
            }

            var measureString = Alex.Font.MeasureString(s);
            while (measureString.X > ButtonRectangle.Width - 13)
            {
                s = s.Remove(0, 1);
                measureString = Alex.Font.MeasureString(s);
            }

            args.SpriteBatch.Begin();

            args.SpriteBatch.Draw(ButtonTexture, ButtonRectangle, Color.Cornsilk);
            args.SpriteBatch.DrawString(Alex.Font, s,
                new Vector2(Location.X + 10, Location.Y + (measureString.Y/2) - 3), color);

            args.SpriteBatch.End();
        }

        private KeyboardState PrevKeyState { get; set; }
        private DateTime LastUpdate { get; set; }
        private DateTime LastChange { get; set; }
        private MouseState PrevMouseState { get; set; }
        public override void Update(GameTime time)
        {
            var ms = Mouse.GetState();
            var mouseRec = new Rectangle(ms.X, ms.Y, 1, 1);

            if (ms != PrevMouseState)
            {
                if (ms.LeftButton == ButtonState.Released && PrevMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (mouseRec.Intersects(ButtonRectangle))
                    {
                        Focus = true;
                    }
                    else
                    {
                        Focus = false;
                    }
                }
            }
            PrevMouseState = ms;

            if (!Focus) return;

            if (DateTime.Now.Subtract(LastUpdate).TotalMilliseconds < 50) return;

            KeyboardState state = Keyboard.GetState();
            if (PrevKeyState != state)
            {
                if (state.IsKeyDown(Keys.Back))
                {
                    if (Text.Length > 0) Text = Text.Remove(Text.Length - 1, 1);
                }
            }
            PrevKeyState = state;
            LastUpdate = DateTime.Now;

            if (DateTime.Now.Subtract(LastChange).TotalMilliseconds >= 500)
            {
                DoThing = !DoThing;
                LastChange = DateTime.Now;
            }
        }
    }
}
