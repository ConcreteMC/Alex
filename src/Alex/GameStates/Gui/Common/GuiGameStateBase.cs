using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.GameStates;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.Common
{
    public class GuiGameStateBase : GuiScreen, IGameState
    {
        protected Alex Alex => Alex.Instance;

        public GuiGameStateBase()
        {
        }

        public IGameState ParentState { get; set; }
        public void Load(IRenderArgs args)
        {
            OnLoad(args);

            //Init(Alex.GuiManager.GuiRenderer);

            InvalidateLayout();
        }

        public void Unload()
        {
            OnUnload();
        }

        public void Draw(IRenderArgs args)
        {
            OnDraw(args);

            //Draw(Alex.GuiManager.GuiSpriteBatch, args.GameTime);
        }
        
        public void Show()
        {
            Alex.GuiManager.AddScreen(this);
            OnShow();
            InvalidateLayout();
        }

        public void Hide()
        {
            OnHide();
            Alex.GuiManager.RemoveScreen(this);
        }

        protected TService GetService<TService>() where TService : class
        {
            return Alex.Services.GetService<TService>();
        }
		
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected virtual void OnLoad(IRenderArgs args) { }
        protected virtual void OnUnload() { }
        protected virtual void OnDraw(IRenderArgs args) { }
    }
}
