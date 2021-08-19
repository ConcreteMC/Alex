using Alex.Common.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Common
{
    public class GuiInGameStateBase : GuiMenuStateBase
    {
        public GuiInGameStateBase()
        {
            Background = null;
            BackgroundOverlay = new Color(Color.Black, 0.65f);
        }

        protected override void OnDraw(IRenderArgs args)
        {
            base.OnDraw(args);
            ParentState?.Draw(args);
        }

        /// <inheritdoc />
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            ParentState?.Update(gameTime);
        }

        protected override void OnShow()
        {
            Alex.IsMouseVisible = true;
            base.OnShow();
        }
        
        /// <inheritdoc />
        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}
