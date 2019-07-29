using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Entities;
using Alex.GameStates;
using Alex.Gamestates.Debug;
using Alex.Graphics.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Elements
{
    public class GuiEntityModelView : GuiElement
    {
        public PlayerLocation EntityPosition
        {
            get { return Entity.KnownPosition; }
            set { Entity.KnownPosition = value; }
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


        public Entity Entity
        {
            get => _entity;
            set { _entity = value; }
        }

        private GuiEntityModelViewCamera Camera { get; }
        private bool                     _canRender;

        public GuiEntityModelView(Entity entity)
        {
            if (entity.ModelRenderer != null)
            {
                _canRender = true;
            }

            Entity         = entity;
            EntityPosition = new PlayerLocation(Vector3.Zero);
            Background     = GuiTextures.PanelGeneric;

            //Camera = new GuiEntityModelViewCamera(this);
            Camera = new GuiEntityModelViewCamera(EntityPosition);
        }


        public void SetEntityRotation(float yaw, float pitch)
        {
            EntityPosition.Yaw   = yaw;
            EntityPosition.Pitch = pitch;
        }

        public void SetEntityRotation(float yaw, float pitch, float headYaw)
        {
            EntityPosition.Yaw     = yaw;
            EntityPosition.Pitch   = pitch;
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
        private Entity    _entity;

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
                    GameTime       = gameTime,
                    Camera         = Camera
                };

                Camera.MoveTo(EntityPosition, Vector3.Zero);

                Entity.Update(updateArgs);
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
                    SpriteBatch    = graphics.SpriteBatch,
                    GameTime       = gameTime,
                    Camera         = Camera,
                };

                //var viewport = args.Graphics.Viewport;
                //args.Graphics.Viewport = new Viewport(RenderBounds);
                //graphics.End();

                using (var context = graphics.BranchContext(BlendState.AlphaBlend, DepthStencilState.Default,
                                                            RasterizerState.CullClockwise, SamplerState.PointWrap))
                {
                    var bounds = RenderBounds;

                    bounds.Inflate(-3, -3);

                    var p  = graphics.Project(bounds.Location.ToVector2());
                    var p2 = graphics.Project(bounds.Location.ToVector2() + bounds.Size.ToVector2());

                    var newViewport = Camera.Viewport;
                    newViewport.X      = (int) p.X;
                    newViewport.Y      = (int) p.Y;
                    newViewport.Width  = (int) (p2.X - p.X);
                    newViewport.Height = (int) (p2.Y - p.Y);

                    Camera.Viewport = newViewport;
                    Camera.UpdateProjectionMatrix();

                    context.Viewport = Camera.Viewport;

                    graphics.Begin();

                    Entity.Render(renderArgs);
                    //  EntityModelRenderer?.Render(renderArgs, EntityPosition);

                    graphics.End();
                }
            }
        }

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

            private GuiEntityModelViewCamera Camera { get; }
            private bool                     _canRender;

            public GuiModelExplorerView(ModelExplorer modelExplorer, Vector3 cameraOffset, Vector3 cameraTargetOffset) : this(modelExplorer, cameraOffset)
            {
                Camera.EntityTargetOffset = cameraTargetOffset;
            }

            public GuiModelExplorerView(ModelExplorer modelExplorer, Vector3 cameraOffset) : this(modelExplorer)
            {
                Camera.EntityPositionOffset = cameraOffset;
            }
            public GuiModelExplorerView(ModelExplorer modelExplorer)
            {
                if (modelExplorer != null)
                {
                    _canRender = true;
                }

                ModelExplorer  = modelExplorer;
                EntityPosition = new PlayerLocation(Vector3.Zero);
                Background     = GuiTextures.PanelGeneric;

                //Camera = new GuiEntityModelViewCamera(this);
                Camera = new GuiEntityModelViewCamera(EntityPosition);
            }


            public void SetEntityRotation(float yaw, float pitch)
            {
                EntityPosition.Yaw   = yaw;
                EntityPosition.Pitch = pitch;
            }

            public void SetEntityRotation(float yaw, float pitch, float headYaw)
            {
                EntityPosition.Yaw     = yaw;
                EntityPosition.Pitch   = pitch;
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

            private Rectangle     _previousBounds;
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
                        GameTime       = gameTime,
                        Camera         = Camera
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
                        SpriteBatch    = graphics.SpriteBatch,
                        GameTime       = gameTime,
                        Camera         = Camera,
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

                        var p  = graphics.Project(bounds.Location.ToVector2());
                        var p2 = graphics.Project(bounds.Location.ToVector2() + bounds.Size.ToVector2());

                        var newViewport = Camera.Viewport;
                        newViewport.X      = (int) p.X;
                        newViewport.Y      = (int) p.Y;
                        newViewport.Width  = (int) (p2.X - p.X);
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

        class GuiEntityModelViewCamera : Camera
            {

                public Viewport Viewport             { get; set; }
                public Vector3  EntityPositionOffset { get; set; } = new Vector3(0f, 0f, -6f);
                public Vector3  EntityTargetOffset { get; set; } = new Vector3(0f, 1.8f, 0f);

                public GuiEntityModelViewCamera(Vector3 basePosition) : base(1)
                {
                    Viewport = new Viewport(256, 128, 128, 256, 0.01f, 16.0f);
                    Position = basePosition;
                    Rotation = Vector3.Zero;
                    FOV      = 25.0f;

                    UpdateAspectRatio(Viewport.AspectRatio);
                }

                protected override void UpdateViewMatrix()
                {
                    Matrix rotationMatrix = (Matrix.CreateRotationX(Rotation.X) *
                                             Matrix.CreateRotationY(Rotation.Y));

                    Vector3 lookAtOffset = Vector3.Transform(EntityPositionOffset, rotationMatrix);

                    Target = Position;

                    Direction = Vector3.Transform(Vector3.Forward, rotationMatrix);

                    ViewMatrix = Matrix.CreateLookAt(Target + lookAtOffset, Target + EntityTargetOffset, Vector3.Up);
                }

                public override void UpdateProjectionMatrix()
                {
                    //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(Viewport.RenderBounds, NearDistance, FarDistance);
                    ProjectionMatrix =
                        Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Viewport.AspectRatio,
                                                            NearDistance, FarDistance);
                }

                //public override void UpdateProjectionMatrix()
                //{
                //    var bounds = _modelView.RenderBounds;
                //    ProjectionMatrix =
                //        Matrix.CreatePerspective(Viewport.Width, Viewport.Height, Viewport.MinDepth, Viewport.MaxDepth);
                //    //ProjectionMatrix = Matrix.CreateOrthographicOffCenter(Viewport.RenderBounds, NearDistance, FarDistance);// * Matrix.CreateTranslation(new Vector3(bounds.Location.ToVector2(), -1f));
                //    //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(_modelView.RenderBounds, NearDistance, FarDistance);
                //}
            }
        }
    
}
