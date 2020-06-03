using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gamestates;
using Alex.Gamestates.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Elements
{
    public class GuiModelExplorerView : GuiElement
    {
        private PlayerLocation _entityLocation;

        public PlayerLocation EntityPosition
        {
            get { return _entityLocation; }
            set
            {
                _entityLocation = value;
                ModelExplorer?.SetLocation(_entityLocation);
            }
        }

        private string _entityName;

        public string EntityName
        {
            get => _entityName;
            set
            {
                _entityName = value;
                InitRenderer();
            }
        }


        public ModelExplorer ModelExplorer
        {
            get => _modelExplorer;
            set
            {
                _modelExplorer = value;
                EntityPosition = new PlayerLocation(Vector3.Zero);
            }
        }

        private GuiContext3DElement.GuiContext3DCamera Camera { get; }
        private bool _canRender;

        public GuiModelExplorerView(ModelExplorer modelExplorer, Vector3 cameraOffset, Vector3 cameraTargetPositionOffset) :
            this(modelExplorer, cameraOffset)
        {
            Camera.TargetPositionOffset = cameraTargetPositionOffset;
        }

        public GuiModelExplorerView(ModelExplorer modelExplorer, Vector3 cameraOffset) : this(modelExplorer)
        {
            Camera.TargetPositionOffset = cameraOffset;
        }

        public GuiModelExplorerView(ModelExplorer modelExplorer)
        {
            if (modelExplorer != null)
            {
                _canRender = true;
            }

            ModelExplorer = modelExplorer;
            EntityPosition = new PlayerLocation(Vector3.Zero);
            Background = GuiTextures.PanelGeneric;

            //Camera = new GuiEntityModelViewCamera(this);
            Camera = new GuiContext3DElement.GuiContext3DCamera(EntityPosition);
        }

        public void SetEntityRotation(float yaw, float pitch)
        {
            EntityPosition.Yaw = yaw;
            EntityPosition.Pitch = pitch;
            //TODO: Check what is correct.
            //ViewMatrix = Matrix.CreateLookAt(Target + lookAtOffset, Target + (Vector3.Up * Player.EyeLevel), Vector3.Up);
        }

        public void SetEntityRotation(float yaw, float pitch, float headYaw)
        {
            EntityPosition.Yaw = yaw;
            EntityPosition.Pitch = pitch;
            EntityPosition.HeadYaw = headYaw;
        }

        private void InitRenderer()
        {
            /*  if (string.IsNullOrWhiteSpace(EntityName) || SkinTexture == null)
              {
                  _canRender = false;
                  EntityModelRenderer = null;
                  return;
              }
              Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue(EntityName, out EntityModel m);

              EntityModelRenderer = new EntityModelRenderer(m, SkinTexture);
              _canRender = true;*/
        }

        private Rectangle _previousBounds;
        private ModelExplorer _modelExplorer;

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_canRender)
            {
                var bounds = Screen.RenderBounds;

                if (bounds != _previousBounds)
                {
                    //var c = bounds.Center;
                    //Camera.RenderPosition = new Vector3(c.X, c.Y, 0.0f);
                    //Camera.UpdateProjectionMatrix();
                    //Camera.UpdateAspectRatio((float) bounds.Width / (float) bounds.Height);
                    Camera.UpdateAspectRatio(bounds.Width / (float) bounds.Height);
                    _previousBounds = bounds;
                }

                var updateArgs = new UpdateArgs()
                {
                    GraphicsDevice = Alex.Instance.GraphicsDevice,
                    GameTime = gameTime,
                    Camera = Camera
                };

                Camera.MoveTo(EntityPosition, Vector3.Zero);
                ModelExplorer.SetLocation(EntityPosition);
                ModelExplorer.Update(updateArgs);
                //EntityModelRenderer?.Update(updateArgs, EntityPosition);
            }
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            if (_canRender)
            {
                var renderArgs = new RenderArgs()
                {
                    GraphicsDevice = graphics.SpriteBatch.GraphicsDevice,
                    SpriteBatch = graphics.SpriteBatch,
                    GameTime = gameTime,
                    Camera = Camera,
                };

                //var viewport = args.Graphics.Viewport;
                //args.Graphics.Viewport = new Viewport(RenderBounds);
                //graphics.End();

                using (var context = graphics.BranchContext(BlendState.AlphaBlend, DepthStencilState.DepthRead,
                    RasterizerState.CullClockwise, SamplerState.PointWrap))
                {
                    //context.GraphicsDevice.BlendFactor = Color.TransparentBlack;
                    var bounds = RenderBounds;

                    bounds.Inflate(-3, -3);

                    var p = graphics.Project(bounds.Location.ToVector2());
                    var p2 = graphics.Project(bounds.Location.ToVector2() + bounds.Size.ToVector2());

                    var newViewport = Camera.Viewport;
                    newViewport.X = (int) p.X;
                    newViewport.Y = (int) p.Y;
                    newViewport.Width = (int) (p2.X - p.X);
                    newViewport.Height = (int) (p2.Y - p.Y);

                    Camera.Viewport = newViewport;
                    Camera.UpdateProjectionMatrix();

                    context.Viewport = Camera.Viewport;

                    graphics.Begin();

                    ModelExplorer.Render(context, renderArgs);
                    //  EntityModelRenderer?.Render(renderArgs, EntityPosition);

                    graphics.End();
                }
            }
        }
    }
}