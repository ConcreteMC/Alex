using System;
using System.Collections.Generic;
using Alex.API.GameStates;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        //private GuiDebugHelper DebugHelper { get; }

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
        
        public GuiManager(Game game, InputManager inputManager, IGuiRenderer guiRenderer)
        {
            Game = game;
            InputManager = inputManager;
            ScaledResolution = new GuiScaledResolution(game);
            ScaledResolution.ScaleChanged += ScaledResolutionOnScaleChanged;

            FocusManager = new GuiFocusHelper(this, InputManager, game.GraphicsDevice);

            GuiRenderer = guiRenderer;
            guiRenderer.ScaledResolution = ScaledResolution;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            GuiSpriteBatch = new GuiSpriteBatch(guiRenderer, Game.GraphicsDevice, SpriteBatch);
            GuiRenderArgs = new GuiRenderArgs(Game.GraphicsDevice, SpriteBatch, ScaledResolution, GuiRenderer, new GameTime());

            //DebugHelper = new GuiDebugHelper(this);
        }

        private void ScaledResolutionOnScaleChanged(object sender, UiScaleEventArgs args)
        {
            Init(Game.GraphicsDevice);
            
            foreach (var screen in Screens.ToArray())
            {
                screen.UpdateSize(args.ScaledWidth, args.ScaledHeight);
            }
        }

        public void Init(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            GuiRenderer.Init(graphicsDevice);
            
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

            //DebugHelper.Update(gameTime);
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
                    //DebugHelper.DrawScreen(screen);
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
