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
                }
                color = Color.Gray;
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

        public override void Update(GameTime time)
        {
            var ms = Mouse.GetState();
            var mouseRec = new Rectangle(ms.X, ms.Y, 1, 1);

            if (ms.LeftButton == ButtonState.Pressed)
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

            if (!Focus) return;

            if (DateTime.Now.Subtract(LastUpdate).TotalMilliseconds < 50) return;

            KeyboardState state = Keyboard.GetState();
            if (PrevKeyState != state)
            {
                var keys = state.GetPressedKeys();
                if (keys.Length > 0)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        var key = keys[i];

                        if (key == Keys.Back)
                        {
                            if (Text.Length > 0) Text = Text.Remove(Text.Length - 1, 1);
                        }
                        else if (key == Keys.Space)
                        {
                            Text += ' ';
                        }
                        else if (IsKeyAChar(key) || IsKeyADigit(key) || key.RepresentsPrintableChar())
                        {
                            char val = (char) key;
                            if (Alex.Font.Characters.Contains(val))
                            {
                                if (keys.Contains(Keys.LeftShift) || keys.Contains(Keys.RightShift))
                                {
                                    var a = GetModifiedKey(val);
                                    if (a == val)
                                    {
                                        val = char.ToUpper(val);
                                    }
                                    else
                                    {
                                        val = a;
                                    }
                                }
                                else
                                {
                                    val = char.ToLower(val);
                                }
                                Text += val;
                            }
                        }
                    }
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

        private static bool IsKeyAChar(Keys key)
        {
            return key >= Keys.A && key <= Keys.Z;
        }

        private static bool IsKeyADigit(Keys key)
        {
            return (key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9);
        }

        [DllImport("user32.dll")]
        static extern short VkKeyScan(char c);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int ToAscii(
            uint uVirtKey,
            uint uScanCode,
            byte[] lpKeyState,
            out uint lpChar,
            uint flags
            );

        private static char GetModifiedKey(char c)
        {
            short vkKeyScanResult = VkKeyScan(c);

            if (vkKeyScanResult == -1)
                return c;

            uint code = (uint)vkKeyScanResult & 0xff;

            byte[] b = new byte[256];
            b[0x10] = 0x80;

            uint r;
            if (1 != ToAscii(code, code, b, out r, 0))
                throw new ApplicationException("Could not translate modified state");

            return (char)r;
        }
    }
}
