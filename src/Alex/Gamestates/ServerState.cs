using System.Net;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class ServerState : Gamestate
    {
        private static readonly string[] AlexLogo =
        {
            "               AAA                lllllll                                        ",
            "              A:::A               l:::::l                                        ",
            "             A:::::A              l:::::l                                        ",
            "            A:::::::A             l:::::l                                        ",
            "           A:::::::::A             l::::l      eeeeeeeeeeee   xxxxxxx      xxxxxxx",
            "          A:::::A:::::A            l::::l    ee::::::::::::ee  x:::::x    x:::::x ",
            "         A:::::A A:::::A           l::::l   e::::::eeeee:::::ee x:::::x  x:::::x  ",
            "        A:::::A   A:::::A          l::::l  e::::::e     e:::::e  x:::::xx:::::x   ",
            "       A:::::A     A:::::A         l::::l  e:::::::eeeee::::::e   x::::::::::x    ",
            "      A:::::AAAAAAAAA:::::A        l::::l  e:::::::::::::::::e     x::::::::x	    ",
            "     A:::::::::::::::::::::A       l::::l  e::::::eeeeeeeeeee      x::::::::x     ",
            "    A:::::AAAAAAAAAAAAA:::::A      l::::l  e:::::::e              x::::::::::x    ",
            "   A:::::A             A:::::A    l::::::l e::::::::e            x:::::xx:::::x   ",
            "  A:::::A               A:::::A   l::::::l  e::::::::eeeeeeee   x:::::x  x:::::x  ",
            " A:::::A                 A:::::A  l::::::l   ee:::::::::::::e  x:::::x    x:::::x ",
            "AAAAAAA                   AAAAAAA llllllll     eeeeeeeeeeeeee xxxxxxx      xxxxxxx"
        };

        private bool _doPlus = true;

        private float _scale = 1.0f;
        private string _splashText = "";

        private Texture2D WoodTexture { get; set; }
        private Texture2D GrassTexture { get; set; }
        private Texture2D BackGround { get; set; }
        public override void Init(RenderArgs args)
        {
            WoodTexture = ResManager.ImageToTexture2D(Properties.Resources.wood);
            GrassTexture = ResManager.ImageToTexture2D(Properties.Resources.grass);
            BackGround = ResManager.ImageToTexture2D(Properties.Resources.mcbg);

            if (_splashText == "") _splashText = SplashTexts.GetSplashText();
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
            Controls["server-ip"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y - 30);
            Controls["server-port"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 20);
            Controls["optbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70);
            Controls["backbtn"].Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 120);

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

            var x = 0;
            var y = 25;
            foreach (var line in AlexLogo)
            {
                foreach (var i in line)
                {
                    float renderX = CenterScreen.X - ((line.Length * 6 / 2) - x);

                    if (i == ':')
                    {
                        args.SpriteBatch.Draw(WoodTexture, new Vector2(renderX, y));
                    }
                    else if (i != ' ')
                    {
                        args.SpriteBatch.Draw(GrassTexture, new Vector2(renderX, y));
                    }

                    x += 6;
                }
                y += 6;
                x = 0;
            }

            float dt = (float)args.GameTime.ElapsedGameTime.TotalSeconds;
            if (_scale > 1.22f)
            {
                _doPlus = false;
            }
            if (_scale < 0.52f)
            {
                _doPlus = true;
            }
            if (_doPlus)
            {
                _scale += 1f * dt;
            }
            else
            {
                _scale -= 1f * dt;
            }


            try
            {
                args.SpriteBatch.DrawString(Alex.Font, _splashText, new Vector2(CenterScreen.X + 186, 140), Color.Gold, -0.6f,
                    new Vector2(),
                    new Vector2(_scale, _scale), 0f, 0f);
            }
            catch
            {
                args.SpriteBatch.DrawString(Alex.Font, "Free bugs for everyone!", new Vector2(CenterScreen.X + 186, 140), Color.Gold,
                    -0.6f, new Vector2(),
                    new Vector2(_scale, _scale), 0f, 0f);
            }

            string text = "Alex - Developed by Kennyvv";
            var size = Alex.Font.MeasureString(text);
            args.SpriteBatch.DrawString(Alex.Font, text, new Vector2(4, (args.GraphicsDevice.Viewport.Height - size.Y) - 2), Color.White);

            args.SpriteBatch.End();
        }

        public override void OnUpdate(GameTime gameTime)
        {

        }
    }
}
