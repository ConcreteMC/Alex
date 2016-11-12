using Alex.Rendering.UI;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class LoginState : Gamestate
    {
        private Texture2D BackGround { get; set; }
        public override void Init(RenderArgs args)
        {
            BackGround = ResManager.ImageToTexture2D(Properties.Resources.mcbg);

            //Alex.ShowMouse();
            Alex.Instance.IsMouseVisible = true;

            Controls.Add("username", new InputField()
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30),
                PlaceHolder = "Username",
                Text = Alex.Username
            });

            //Controls.Add("password", new InputField()
            //{
           //     Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20),
           //     PlaceHolder = "Password",
           //     PasswordField = true,
           // });

            Button opton = new Button("Login")
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20),
            };
            opton.OnButtonClick += Opton_OnButtonClick;
            
            Controls.Add("optbtn", opton);

            Controls.Add("logo", new Logo());
            Controls.Add("info", new Info());
        }

        private void Opton_OnButtonClick()
        {
            var username = (InputField) Controls["username"];
            if (username.Text != string.Empty)
            {
                Alex.Username = username.Text;
                Alex.Instance.SaveSettings();
                Alex.Instance.SetGameState(new MenuState());
            }
            else
            {
                ErrorText = "Username cannot be empty!";
            }
        }

        public override void Stop()
        {
            //Alex.HideMouse();
            Alex.Instance.IsMouseVisible = false;
        }

        private string ErrorText = string.Empty;
        public override void Render2D(RenderArgs args)
        {
            args.SpriteBatch.Begin();

            //Start draw background
            var retval = new Rectangle(
                args.SpriteBatch.GraphicsDevice.Viewport.X,
                args.SpriteBatch.GraphicsDevice.Viewport.Y,
                args.SpriteBatch.GraphicsDevice.Viewport.Width,
                args.SpriteBatch.GraphicsDevice.Viewport.Height);
            args.SpriteBatch.Draw(BackGround, retval, Color.White);
            //End draw backgroun

            if (ErrorText != string.Empty)
            {
                var meisure = Alex.Font.MeasureString(ErrorText);
                args.SpriteBatch.DrawString(Alex.Font, ErrorText,
                    new Vector2((int) (CenterScreen.X - (meisure.X / 2)), (int) CenterScreen.Y - (30 + meisure.Y + 5)), Color.Red);
            }

            args.SpriteBatch.End();
        }

        public override void OnUpdate(GameTime gameTime)
        {
            Controls["username"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30);
            Controls["optbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20);
        }
    }
}
