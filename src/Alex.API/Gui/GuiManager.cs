using System.Collections.Generic;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui
{
    public class GuiManager
    {
        private GuiDebugHelper DebugHelper { get; }

        private Game Game { get; }
        private GraphicsDevice GraphicsDevice { get; set; }

        public GuiScaledResolution ScaledResolution { get; }
        public GuiFocusManager FocusManager { get; }

        public IGuiRenderer GuiRenderer { get; }

        internal InputManager InputManager { get; }
        internal SpriteBatch SpriteBatch { get; private set; }
        internal GuiRenderArgs GuiRenderArgs { get; private set; }

        public List<GuiScreen> Screens { get; } = new List<GuiScreen>();
        
        public GuiManager(Game game, InputManager inputManager, IGuiRenderer guiRenderer)
        {
            Game = game;
            InputManager = inputManager;
            ScaledResolution = new GuiScaledResolution(game);
            ScaledResolution.ScaleChanged += ScaledResolutionOnScaleChanged;

            FocusManager = new GuiFocusManager(this, InputManager, game.GraphicsDevice);

            GuiRenderer = guiRenderer;
            guiRenderer.ScaledResolution = ScaledResolution;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            GuiRenderArgs = new GuiRenderArgs(GuiRenderer, Game.GraphicsDevice, SpriteBatch, new GameTime(), ScaledResolution);

            DebugHelper = new GuiDebugHelper(this);
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

            GuiRenderArgs = new GuiRenderArgs(GuiRenderer, GraphicsDevice, SpriteBatch, new GameTime(), ScaledResolution);
        }

        private bool _doInit = true;
        public void ApplyFont(IFont font)
        {
            GuiRenderer.Font = font;

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
                screen.Update(gameTime);
            }

            DebugHelper.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            var args = GuiRenderArgs;
            try
            {
                //SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None,  RasterizerState.CullNone, null, ScaledResolution.TransformMatrix);
                args.BeginSpriteBatch();

                foreach (var screen in Screens.ToArray())
                {
                    screen.Draw(args);

                    DebugHelper.DrawScreen(screen);
                }
            }
            finally
            {
                args.EndSpriteBatch();
            }
        }

    }
}
