using System;
using System.Linq;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class DisconnectedState : Gamestate
    {
        private string Reason { get; }
        private Texture2D BackGround { get; set; }
        private Alex Alex { get; }
        public DisconnectedState(Alex alex, string reason) : base(alex.GraphicsDevice)
        {
            Alex = alex;
            Reason = reason;
        }

        public override void Init(RenderArgs args)
        {
            Alex.IsMouseVisible = true;
            BackGround = ResManager.ImageToTexture2D(Properties.Resources.mcbg);
            Button opton = new Button("Return to menu")
            {
                Location = new Vector2((int)(CenterScreen.X - 200), (int)CenterScreen.Y + 70),
            };
            opton.OnButtonClick += OptonOnOnButtonClick;

            Controls.Add("optbtn", opton);
            Controls.Add("info", new Info());

            base.Init(args);
        }

        private void OptonOnOnButtonClick()
        {
            Alex.GamestateManager.SetActiveState("menu");
        }

        public override void Render2D(RenderArgs args)
        {
            try
            {
                args.SpriteBatch.Begin();

                var retval = new Rectangle(
                    args.SpriteBatch.GraphicsDevice.Viewport.X,
                    args.SpriteBatch.GraphicsDevice.Viewport.Y,
                    args.SpriteBatch.GraphicsDevice.Viewport.Width,
                    args.SpriteBatch.GraphicsDevice.Viewport.Height);
                args.SpriteBatch.Draw(BackGround, retval, Color.White);

                const string msg = "Disconnected from server:";
                var size = Alex.Font.MeasureString(msg);
                args.SpriteBatch.DrawString(Alex.Font, msg,
                    new Vector2(CenterScreen.X - (size.X/2), CenterScreen.Y - (size.Y*2)), Color.White);

                float lastY = 0;
                var split = Reason.Split('\n');
                for (int index = 0; index < split.Length; index++)
                {
                    var message = split[index].StripColors();
                    message = message.StripIllegalCharacters();

                    try
                    {
                        lastY = CenterScreen.Y + (size.Y*(index + 1));
                        var size2 = Alex.Font.MeasureString(message);
                        args.SpriteBatch.DrawString(Alex.Font, message, new Vector2(CenterScreen.X - (size2.X/2), lastY),
                            Color.White);
                    }
                    catch
                    {
                    }
                }

                Controls["optbtn"].Location = new Vector2((int) (CenterScreen.X - 200), lastY + 50);
            }
            finally
            {
                args.SpriteBatch.End();
            }
        }
    }
}
