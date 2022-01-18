using Microsoft.Xna.Framework;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Attributes;
using RocketUI;

namespace ResourcePackLib.ModelExplorer.Scenes
{
    public abstract class GuiSceneBase<TScreen> : GuiSceneBase where TScreen : Screen, new()
    {
        protected GuiSceneBase() : this(new TScreen()) { }
        
        protected GuiSceneBase(TScreen screen) : base()
        {
            screen = screen;
            screen.Anchor = Alignment.Fill;
            screen.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Screen = screen;
        }
    }
    
    public abstract class GuiSceneBase : Scene
    {
        private Screen    _screen;

        public Screen Screen
        {
            get => _screen;
            set => SetupScreen(value);
        }
        
        [Service] protected GuiManager GuiManager { get; private set; }
        
        private void SetupScreen(Screen screen)
        {
            if (_screen != null && _screen != screen)
            {
                GuiManager.RemoveScreen(_screen);
            }
            _screen = screen;
            if (screen == null) return;

            screen.Tag = this;
            screen.IsSelfManaged = false;
            //screen.Background = (Color.Black * 0.2f);
            screen.ClipToBounds = true;
            //screen.UpdateSize(screen.Width, screen.Height);

            if(Visible)
                GuiManager.AddScreen(_screen);
        }

        protected override void OnShow()
        {
            GuiManager.AddScreen(_screen);
        }

        protected override void OnHide()
        {
            if (_screen != null && GuiManager != null)
            {
                GuiManager.RemoveScreen(_screen);
            }
        }
    }
}