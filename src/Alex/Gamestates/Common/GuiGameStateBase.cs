using Alex.API.GameStates;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using RocketUI;

namespace Alex.Gamestates.Common
{
    public class GuiGameStateBase : GuiScreen, IGameState
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GuiGameStateBase));
	    
        protected Alex Alex => Alex.Instance;
		

		public bool IsLoaded { get; private set; }
		public bool IsShown  { get; private set; }

		public IGameState ParentState { get; set; }

        public GuiGameStateBase()
        {
	        GuiTextElement cc;
	        AddChild(cc = new GuiTextElement()
	        {
		        Anchor = Alignment.BottomLeft,
		        Text = "github.com/kennyvv/Alex",
		        TextColor = TextColor.White,
		        TextOpacity = 0.5f,
		        Scale = 0.5f,
		        Margin = new Thickness(5, 0, 0, 5)
	        });
	        AddChild(new GuiTextElement()
	        {
		        Anchor = Alignment.BottomRight,
		        Text = "Not affiliated with Mojang/Minecraft",
		        TextColor = TextColor.White,
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
			if (!IsLoaded) return;
			IsLoaded = false;
            OnUnload();
        }

        public void Draw(IRenderArgs args)
        {
            OnDraw(args);

            //Draw(Alex.GuiManager.GuiSpriteBatch, args.GameTime);
        }
        
        public void Show()
        {
			if(IsShown) return;
			
	        Alex.GuiManager.AddScreen(this);
            OnShow();
			
			IsShown = true;

            InvalidateLayout();
        }

        public void Hide()
        {
			if(!IsShown) return;
			IsShown = false;

	        OnHide();
            Alex.GuiManager.RemoveScreen(this);
		}

        public TService GetService<TService>() where TService : class
        {
            return Alex.Services.GetRequiredService<TService>();
        }
		
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected virtual void OnLoad(IRenderArgs args) { }
        protected virtual void OnUnload() { }
        protected virtual void OnDraw(IRenderArgs args) { }
    }
}
