using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.GameStates;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Services;
using Alex.Graphics.VR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Blocks;
using SharpVR;

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

        public bool VrModeEnabled { get; set; }
        private RenderTarget2D _vrGuiBaseTarget { get; set; }
        public ICameraWrapper CameraWrapper { get; set; }

        internal VrService VrService { get; private set; }
        
        private readonly VrContext _vrContext;
        private VrGuiCamera _camera;

        public GuiManager(Game game,
            IServiceProvider serviceProvider,
            InputManager inputManager,
            IGuiRenderer guiRenderer,
            IOptionsProvider optionsProvider,
            bool vrModeEnabled = false
        )
        {
            Game = game;
            ServiceProvider = serviceProvider;
            InputManager = inputManager;
            ScaledResolution = new GuiScaledResolution(game)
            {
                GuiScale = optionsProvider.AlexOptions.VideoOptions.GuiScale
            };
            ScaledResolution.ScaleChanged += ScaledResolutionOnScaleChanged;
            _camera = new VrGuiCamera()
            {
                Position = new Vector3(0, 0, -10f)
            };

            FocusManager = new GuiFocusHelper(this, InputManager, game.GraphicsDevice);

            GuiRenderer = guiRenderer;
            guiRenderer.ScaledResolution = ScaledResolution;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            GuiSpriteBatch = new GuiSpriteBatch(guiRenderer, Game.GraphicsDevice, SpriteBatch);
            GuiRenderArgs = new GuiRenderArgs(Game.GraphicsDevice, SpriteBatch, ScaledResolution, GuiRenderer,
                new GameTime());
            //  DebugHelper = new GuiDebugHelper(this);

            optionsProvider.AlexOptions.VideoOptions.GuiScale.Bind((value, newValue) =>
            {
                ScaledResolution.GuiScale = newValue;
            });

            VrModeEnabled = vrModeEnabled;
            if(vrModeEnabled)
                _vrContext = serviceProvider.GetService<VrContext>();
        }

        private void ScaledResolutionOnScaleChanged(object sender, UiScaleEventArgs args)
        {
            Init(Game.GraphicsDevice, ServiceProvider);
            SetSize(args.ScaledWidth, args.ScaledHeight);
        }

        public void SetSize(int width, int height)
        {
            foreach (var screen in Screens.ToArray())
            {
                screen.UpdateSize(width, height);
            }
        }

        public void Init(GraphicsDevice graphicsDevice, IServiceProvider serviceProvider)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            GuiRenderer.Init(graphicsDevice, serviceProvider);

            GuiSpriteBatch?.Dispose();
            GuiSpriteBatch = new GuiSpriteBatch(GuiRenderer, graphicsDevice, SpriteBatch);
            GuiRenderArgs =
                new GuiRenderArgs(GraphicsDevice, SpriteBatch, ScaledResolution, GuiRenderer, new GameTime());

            VrService = serviceProvider.GetService<VrService>();
            
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

            if (ActiveDialog != null) RemoveScreen(ActiveDialog);
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
                    if (ActiveDialog == dialog)
                        ActiveDialog = Screens.ToArray().LastOrDefault(e => e is TGuiDialog) as GuiDialogBase;
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
            _camera.Update(null);
            ScaledResolution.Update();
            
            var screens = Screens.ToArray();

            if (_doInit)
            {
                _doInit = false;

                foreach (var screen in screens)
                {
                    screen?.Init(GuiRenderer, true);
                }
            }

            FocusManager.Update(gameTime);

            foreach (var screen in screens)
            {
                if (screen == null || screen is IGameState)
                    continue;

                screen.Update(gameTime);
            }

            // DebugHelper.Update(gameTime);
        }

        private void EnsureGuiRenderTarget()
        {
            if (_vrGuiBaseTarget == null)
            {
                _vrGuiBaseTarget = new RenderTarget2D(GraphicsDevice, ScaledResolution.ScaledWidth,
                    ScaledResolution.ScaledHeight);
            }

            if (_basicEffect == null)
            {
                _basicEffect = new BasicEffect(GraphicsDevice)
                {
                    //World = Matrix.CreateConstrainedBillboard(Vector3.Zero, _camera.Position, Vector3.Forward, _camera.Forward, null)
                };
            }
        }

        private int i = 0;

        private BasicEffect _basicEffect;
        
        public void Draw(GameTime gameTime)
        {
            IDisposable maybeADisposable = null;

            bool vrEnabled = VrModeEnabled;


            try
            {
                //if (vrEnabled)
                {
                    //CameraWrapper.PreDraw(_camera);
                    //EnsureGuiRenderTarget();
                    //GraphicsDevice.SetRenderTarget(_vrGuiBaseTarget);
                    // maybeADisposable =
                    //     GuiSpriteBatch.BeginTransform(Matrix.CreateTranslation(-(ScaledResolution.ScaledWidth / 2f), 0,
                    //         15));
                    //maybeADisposable =
                    //    GuiSpriteBatch.BeginTransform(Matrix.CreateTranslation(1, 1, 1));

                    //var pos = CameraWrapper.Position + (CameraWrapper.Forward * 10f);

                    //GuiSpriteBatch.Effect.Projection = CameraWrapper.Projection;
                    //GuiSpriteBatch.Effect.View = CameraWrapper.View;
                    // GuiSpriteBatch.Effect.World = Matrix.Identity
                    //                               //* Matrix.CreateScale(ScaledResolution.ScaleFactor)
                    //                               * Matrix.CreateBillboard(pos, CameraWrapper.Position, Vector3.Up,
                    //                                   CameraWrapper.Forward)
                    // * Matrix.CreateTranslation(
                    //     25*MathF.Sin(MathHelper.ToRadians(i/8f)), 
                    //     25*MathF.Sin(MathHelper.ToRadians(i/4f)), 
                    //     25*MathF.Cos(MathHelper.ToRadians(i/2f)));
                    ;
                    //i++;
                    // var size = ScaledResolution.ViewportSize;
                    // GuiSpriteBatch.Effect.Projection = CameraWrapper.Projection;
                    // GuiSpriteBatch.Effect.View = CameraWrapper.View;
                    // GuiSpriteBatch.Effect.World =
                    //     // Matrix.CreateWorld(Vector3.Zero, Vector3.Forward,
                    //     //     Vector3.Up)
                    //     Matrix.CreateBillboard(, Vector3.Zero, Vector3.Up, CameraWrapper.Forward, null)
                    //     * ScaledResolution.TransformMatrix;
                    //*Matrix.CreateScale(1 / (16f / 16f));
                    // GuiSpriteBatch.Effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * 
                    //                                    Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
                    //

                    GuiSpriteBatch.Effect.Projection = VrService.GetProjectionMatrix();
                    GuiSpriteBatch.Effect.View = VrService.GetViewMatrix(_camera.ViewMatrix);
                    GuiSpriteBatch.Effect.World = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(0, 0f, 0f) * Matrix.CreateRotationZ(MathHelper.PiOver2);
                    //maybeADisposable = GraphicsDevice.PushRenderTarget(_vrGuiBaseTarget);
                    //GuiSpriteBatch.Effect = null;
                    GuiSpriteBatch.Begin();
                }
                // else
                // {
                //     // GuiSpriteBatch.Effect.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) *
                //     //                                    Matrix.CreateOrthographicOffCenter(0,
                //     //                                        GraphicsDevice.Viewport.Width,
                //     //                                        GraphicsDevice.Viewport.Height, 0, 0, 1);
                //     GuiSpriteBatch.Begin();
                // }

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
                if (vrEnabled)
                {
                    if (maybeADisposable != null)
                    {
                        maybeADisposable.Dispose();

                        _basicEffect.View = CameraWrapper.View;
                        _basicEffect.Projection = CameraWrapper.Projection;
                        var pos = (Vector3.Right * _vrGuiBaseTarget.Width / 2f)
                                  + (Vector3.Up * _vrGuiBaseTarget.Height / 2f);
//                                  + (Vector3.Backward * (Math.Max(_vrGuiBaseTarget.Width, _vrGuiBaseTarget.Height)));

                        _basicEffect.World = Matrix.CreateScale(0.5f)
                                             * Matrix.CreateTranslation(pos);

                        SpriteBatch.Begin();
                        var bounds = new Rectangle(_vrGuiBaseTarget.Bounds.Location, _vrGuiBaseTarget.Bounds.Size);
                            bounds.Inflate(_vrGuiBaseTarget.Width * -0.1f,
                            _vrGuiBaseTarget.Height * -0.1f);
                        SpriteBatch.Draw(_vrGuiBaseTarget, bounds, Color.White);
                        SpriteBatch.End();
                    }
                    //GraphicsDevice.SetRenderTarget(null);
                }

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