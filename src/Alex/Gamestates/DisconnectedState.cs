using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
    public class DisconnectedState : Gamestate
    {
        private string Reason { get; }
        private Texture2D BackGround { get; set; }
        public DisconnectedState(string reason)
        {
            Reason = reason;
        }

        public override void Init(RenderArgs args)
        {
            BackGround = ResManager.ImageToTexture2D(Properties.Resources.mcbg);
            base.Init(args);
        }

        public override void Render2D(RenderArgs args)
        {
            args.SpriteBatch.Begin();

            var retval = new Rectangle(
              args.SpriteBatch.GraphicsDevice.Viewport.X,
              args.SpriteBatch.GraphicsDevice.Viewport.Y,
              args.SpriteBatch.GraphicsDevice.Viewport.Width,
              args.SpriteBatch.GraphicsDevice.Viewport.Height);
            args.SpriteBatch.Draw(BackGround, retval, Color.White);

            var msg = "Disconnected from server: " + Reason;
            args.SpriteBatch.DrawString(Alex.Font, msg, Vector2.Zero, Color.White);

            args.SpriteBatch.End();
        }
    }
}
