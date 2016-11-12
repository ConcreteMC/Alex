using System.Net;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class ServerState : Gamestate
    {
        private Texture2D BackGround { get; set; }
        public override void Init(RenderArgs args)
        {
            BackGround = ResManager.ImageToTexture2D(Properties.Resources.mcbg);
            //Alex.ShowMouse();
            Alex.Instance.IsMouseVisible = true;

            Controls.Add("server-ip", new InputField()
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30),
                PlaceHolder = "Server address",
            });

            Controls.Add("server-port", new InputField()
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20),
                PlaceHolder = "19132",
                Text = "19132"
            });

            Button opton = new Button("Connect to server")
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70),
            };
            opton.OnButtonClick += Opton_OnButtonClick;

            Controls.Add("optbtn", opton);

            Button backbton = new Button("Back")
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 120),
            };
            backbton.OnButtonClick += backBton_OnButtonClick;

            Controls.Add("backbtn", backbton);

            Controls.Add("logo", new Logo());
            Controls.Add("info", new Info());
        }

        private void Opton_OnButtonClick()
        {
            InputField ip = (InputField) Controls["server-ip"];
            InputField port = (InputField)Controls["server-port"];

            if (ip.Text == string.Empty)
            {
                ErrorText = "Enter a server address";
                return;
            }

            if (port.Text == string.Empty)
            {
                ErrorText = "Enter a server port";
                return;
            }

            //TODO: Connect to server
            Alex.IsMultiplayer = true;
            try
            {
                Alex.ServerEndPoint = new IPEndPoint(ResolveAddress(ip.Text), int.Parse(port.Text));
            }
            catch
            {
                return;
            }

            Alex.Instance.SetGameState(new PlayingState());
        }

        private static IPAddress ResolveAddress(string address)
        {
            IPAddress outAddress;
            if (IPAddress.TryParse(address, out outAddress))
            {
                return outAddress;
            }
            return Dns.GetHostEntry(address).AddressList[0];
        }

        private void backBton_OnButtonClick()
        {
            Alex.Instance.SetGameState(new MenuState());
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
                    new Vector2((int)(CenterScreen.X - (meisure.X / 2)), (int)CenterScreen.Y - (30 + meisure.Y + 5)), Color.Red);
            }

            args.SpriteBatch.End();
        }

        public override void OnUpdate(GameTime gameTime)
        {
            Controls["server-ip"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30);
            Controls["server-port"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20);
            Controls["optbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70);
            Controls["backbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 120);
        }
    }
}
