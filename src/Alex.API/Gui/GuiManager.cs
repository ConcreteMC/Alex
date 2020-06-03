using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.GameStates;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui
{
    public class GuiDrawScreenEventArgs : EventArgs
    {
        public GuiScreen Screen { get; }

        public GameTime GameTime { get; }

        internal GuiDrawScreenEventArgs(GuiScreen screen, GameTime gameTime)
        {
            Screen = screen;
            GameTime = gameTime;
        }
    }

    public class GuiManager
    {
       // public GuiDebugHelper DebugHelper { get; }

        public event EventHandler<GuiDrawScreenEventArgs> DrawScreen;

        private Game Game { get; }
        private GraphicsDevice GraphicsDevice { get; set; }

        public GuiScaledResolution ScaledResolution { get; }
        public GuiFocusHelper FocusManager { get; }

        public IGuiRenderer GuiRenderer { get; }

        internal InputManager InputManager { get; }
        internal SpriteBatch SpriteBatch { get; private set; }
        internal GuiRenderArgs GuiRenderArgs { get; private set; }

        public GuiSpriteBatch GuiSpriteBatch { get; private set; }

        public List<GuiScreen> Screens { get; } = new List<GuiScreen>();

        public GuiDialogBase ActiveDialog { get; private set; }
        
        private IServiceProvider ServiceProvider { get; }
        public GuiManager(Game game, IServiceProvider serviceProvider, InputManager inputManager, IGuiRenderer guiRenderer, IOptionsProvider optionsProvider)
        {
            Game = game;
            ServiceProvider = serviceProvider;
            InputManager = inputManager;
            ScaledResolution = new GuiScaledResolution(game)
            {
                GuiScale = optionsProvider.AlexOptions.VideoOptions.GuiScale
            };
            ScaledResolution.ScaleChanged += ScaledResolutionOnScaleChanged;

            FocusManager = new GuiFocusHelper(this, InputManager, game.GraphicsDevice);

            GuiRenderer = guiRenderer;
            guiRenderer.ScaledResolution = ScaledResolution;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            GuiSpriteBatch = new GuiSpriteBatch(guiRenderer, Game.GraphicsDevice, SpriteBatch);
            GuiRenderArgs = new GuiRenderArgs(Game.GraphicsDevice, SpriteBatch, ScaledResolution, GuiRenderer, new GameTime());

          //  DebugHelper = new GuiDebugHelper(this);

          optionsProvider.AlexOptions.VideoOptions.GuiScale.Bind((value, newValue) =>
              {
                  ScaledResolution.GuiScale = newValue;
              });
        }

        private void ScaledResolutionOnScaleChanged(object sender, UiScaleEventArgs args)
        {
            Init(Game.GraphicsDevice, ServiceProvider);
            
            foreach (var screen in Screens.ToArray())
            {
                screen.UpdateSize(args.ScaledWidth, args.ScaledHeight);
            }
        }

        public void Init(GraphicsDevice graphicsDevice, IServiceProvider serviceProvider)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            GuiRenderer.Init(graphicsDevice, serviceProvider);
            
            GuiSpriteBatch?.Dispose();
            GuiSpriteBatch = new GuiSpriteBatch(GuiRenderer, graphicsDevice, SpriteBatch);
            GuiRenderArgs = new GuiRenderArgs(GraphicsDevice, SpriteBatch, ScaledResolution, GuiRenderer, new GameTime());
        }

        private bool _doInit = true;
        public void ApplyFont(IFont font)
        {
            GuiRenderer.Font = font;
            GuiSpriteBatch.Font = font;

            _doInit = true;
        }

        public void ShowDialog(GuiDialogBase dialog)
        {
            ActiveDialog?.OnClose();
            
            if(ActiveDialog != null) RemoveScreen(ActiveDialog);
            ActiveDialog = dialog;
            AddScreen(ActiveDialog);

            Game.IsMouseVisible = true;
        }

        public void HideDialog(GuiDialogBase dialog)
        {
            if (ActiveDialog == dialog)
            {
                dialog?.OnClose();
                
                Game.IsMouseVisible = false;
                Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
                
                RemoveScreen(ActiveDialog);
                
                ActiveDialog = null;
            }
        }

        public void HideDialog<TGuiDialog>() where TGuiDialog : GuiDialogBase
        {
            foreach (var screen in Screens.ToArray())
            {
                if (screen is TGuiDialog dialog)
                {
                    dialog?.OnClose();
                    Screens.Remove(dialog);
                    if(ActiveDialog == dialog) ActiveDialog = Screens.ToArray().LastOrDefault(e => e is TGuiDialog) as GuiDialogBase;
                }
            }
        }
        
        public void AddScreen(GuiScreen screen)
        {
            screen.Init(GuiRenderer);
            screen.UpdateSize(ScaledResolution.ScaledWidth, ScaledResolution.ScaledHeight);
            Screens.Add(screen);
        }

        public void RemoveScreen(GuiScreen screen)
        {
            Screens.Remove(screen);
        }

	    public bool HasScreen(GuiScreen screen)
	    {
		    return Screens.Contains(screen);
	    }

        public void Update(GameTime gameTime)
        {
            ScaledResolution.Update();

            var screens = Screens.ToArray();

            if (_doInit)
            {
                _doInit = false;
                
                foreach (var screen in screens)
                {
                    screen.Init(GuiRenderer, true);
                }
            }

            FocusManager.Update(gameTime);

            foreach (var screen in screens)
            {
                if (!(screen is IGameState) && screen != null)
                {
                    screen.Update(gameTime);
                }
            }

           // DebugHelper.Update(gameTime);
        }
        
        public void Draw(GameTime gameTime)
        {
            try
            {
                GuiSpriteBatch.Begin();

                ForEachScreen(screen =>
                {
					screen.Draw(GuiSpriteBatch, gameTime);

                    DrawScreen?.Invoke(this, new GuiDrawScreenEventArgs(screen, gameTime));
                  //  DebugHelper.DrawScreen(screen);
                });
            }
            finally
            {
                GuiSpriteBatch.End();
            }
        }


        private void ForEachScreen(Action<GuiScreen> action)
        {
            foreach (var screen in Screens.ToArray())
            {
	            if (screen == null) continue;
                action.Invoke(screen);
            }
        }
    }
}
