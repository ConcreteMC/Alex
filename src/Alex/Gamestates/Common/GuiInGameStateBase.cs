using Alex.API.Graphics;
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
            ParentState.Draw(args);
        }

        protected override void OnShow()
        {
            Alex.IsMouseVisible = true;
            base.OnShow();
        }
    }
}
