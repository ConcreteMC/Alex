using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.Common
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
