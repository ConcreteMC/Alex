using Alex.Gamestates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering.UI
{
    public class Logo : UIComponent
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

        public Logo()
        {
            WoodTexture = ResManager.ImageToTexture2D(Properties.Resources.wood);
            GrassTexture = ResManager.ImageToTexture2D(Properties.Resources.grass);

            if (_splashText == "") _splashText = SplashTexts.GetSplashText();
        }

        private Vector2 CenterScreen
        {
            get
            {
                return new Vector2((Alex.Instance.GraphicsDevice.Viewport.Width / 2f),
                    (Alex.Instance.GraphicsDevice.Viewport.Height / 2f));
            }
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
        }

        public override void Render(RenderArgs args)
        {
            args.SpriteBatch.Begin();

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

            args.SpriteBatch.End();
        }
    }
}
