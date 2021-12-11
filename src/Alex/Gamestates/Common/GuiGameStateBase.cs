using System;
using Alex.Common.Data.Options;
using Alex.Common.GameStates;
using Alex.Common.Graphics;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Common
{
    public class GuiGameStateBase : Screen, IGameState
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GuiGameStateBase));
	    private IGameState _parentState;

	    protected Alex Alex => Alex.Instance;
        public AlexOptions Options => GetService<IOptionsProvider>().AlexOptions;

		public bool IsLoaded { get; private set; }
		public bool IsShown  { get; private set; }

		/// <inheritdoc />
		public StateBehavior StateBehavior { get; set; }

		public IGameState ParentState
		{
			get => _parentState;
			set
			{
				_parentState = value;
			}
		}

		public GuiGameStateBase()
        {
	        //IsSelfManaged = true;
	        
	        TextElement cc;
	        AddChild(cc = new TextElement()
	        {
		        Anchor = Alignment.BottomLeft,
		        Text = "github.com/kennyvv/Alex",
		        TextColor = (Color) TextColor.White,
		        TextOpacity = 0.5f,
		        Scale = 0.5f,
		        Margin = new Thickness(5, 0, 0, 5)
	        });
	        AddChild(new TextElement()
	        {
		        Anchor = Alignment.BottomRight,
		        Text = "Not affiliated with Mojang/Minecraft",
		        TextColor = (Color) TextColor.White,
		        TextOpacity = 0.5f,
		        Scale = 0.5f,
		        Margin = new Thickness(0, 0, 5, 5)
	        });
		}
        public void Load(IRenderArgs args)
        {
			if(IsLoaded) return;
			IsLoaded = true;
            OnLoad(args);

            //Init(Alex.GuiManager.GuiRenderer);

            InvalidateLayout();
        }

        public void Unload()
		{
			//if (!IsLoaded) return;
			IsLoaded = false;
            OnUnload();
        }

        void IGameState.Update(GameTime gameTime)
        {
	        ParentState?.Update(gameTime);
	        
	        if (!Alex.GuiManager.HasScreen(this)) 
				OnUpdate(gameTime);
        }
        
        void IGameState.Draw(IRenderArgs args)
        {
	        ParentState?.Draw(args);
            OnDraw(args);

            //Draw(Alex.GuiManager.GuiSpriteBatch, args.GameTime);
        }

        public void Show()
        {
			if(IsShown) return;
			IsShown = true;
			
			if (!Alex.InGame)
				Alex.SetFrameRateLimiter(true, 60);
			
			if (!Alex.GuiManager.HasScreen(this)) 
				Alex.GuiManager.AddScreen(this);
			
			Alex.IsMouseVisible = true;
			
            OnShow();

            InvalidateLayout();
        }

        public void Hide()
        {
			if(!IsShown) return;
			IsShown = false;
			
			if (Alex.GuiManager.HasScreen(this)) 
				Alex.GuiManager.RemoveScreen(this);
			
			Alex.IsMouseVisible = false;
			
			OnHide();

			//Alex.ResetFrameRateLimiter();
        }

        public TService GetService<TService>() where TService : class
        {
            return Alex.ServiceContainer.GetRequiredService<TService>();
        }
		
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected virtual void OnLoad(IRenderArgs args) { }
        protected virtual void OnUnload() { }
        protected virtual void OnDraw(IRenderArgs args) { }
    }
}
