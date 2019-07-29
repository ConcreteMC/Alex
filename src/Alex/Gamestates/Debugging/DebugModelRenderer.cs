using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.Gamestates.Debug;
using Alex.Graphics.Camera;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates.Debugging
{
    public class DebugModelRenderer : GuiElement
    {
        public ModelExplorer ModelExplorer { get; set; }
        private DebugModelRendererCamera Camera { get; }// = new FirstPersonCamera(16, Vector3.Zero, Vector3.Zero);
       // private World World { get; }
        private BlockModelExplorer BlockModelExplorer { get; }
        private EntityModelExplorer EntityModelExplorer { get; }
        public DebugModelRenderer(Alex alex)
        {
            EntityPosition = new PlayerLocation(Vector3.Zero);
            Camera = new DebugModelRendererCamera(this);

            // World = new World(alex, alex.GraphicsDevice, alex.Services.GetService<IOptionsProvider>().AlexOptions,
           //     new FirstPersonCamera(16, Vector3.Zero, Vector3.Zero), new DebugNetworkProvider());
            
            BlockModelExplorer = new BlockModelExplorer(alex, null);
            EntityModelExplorer = new EntityModelExplorer(alex, null);

            ModelExplorer = BlockModelExplorer;
        }
        
        public void SwitchModelExplorers()
        {
            if (ModelExplorer == BlockModelExplorer)
            {
                ModelExplorer = EntityModelExplorer;
            }
            else if (ModelExplorer == EntityModelExplorer)
            {
                ModelExplorer = BlockModelExplorer;
            }
        }
        
        private Rectangle _previousBounds;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            
            var bounds = Screen.RenderBounds;

            if (bounds != _previousBounds)
            {
                //var c = bounds.Center;
                //Camera.RenderPosition = new Vector3(c.X, c.Y, 0.0f);
                //Camera.UpdateProjectionMatrix();
                //Camera.UpdateAspectRatio((float) bounds.Width / (float) bounds.Height);
                Camera.UpdateAspectRatio(bounds.Width / (float)bounds.Height);
                _previousBounds = bounds;
            }

            var updateArgs = new UpdateArgs()
            {
                GraphicsDevice = Alex.Instance.GraphicsDevice,
                GameTime       = gameTime,
                Camera         = Camera
            };

            ModelExplorer.SetLocation(EntityPosition);
            Camera.MoveTo(EntityPosition, Vector3.Zero);

            ModelExplorer?.Update(updateArgs);
            //Entity.Update(updateArgs);
            //EntityModelRenderer?.Update(updateArgs, EntityPosition);
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            var renderArgs = new RenderArgs()
            {
                GraphicsDevice = graphics.SpriteBatch.GraphicsDevice,
                SpriteBatch    = graphics.SpriteBatch,
                GameTime       = gameTime,
                Camera         = Camera,
            };
            ClipToBounds = false;
            using (var context = graphics.BranchContext(BlendState.AlphaBlend, new DepthStencilState()
                {
                    DepthBufferEnable = true,
                    StencilEnable = true
                },
                new RasterizerState()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid
                }, SamplerState.PointClamp))
            {
                var bounds = RenderBounds;
                
                bounds.Inflate(-3, -3);

                var p = graphics.Project(bounds.Location.ToVector2());
                var p2 = graphics.Project(bounds.Location.ToVector2() + bounds.Size.ToVector2());
                    
                var newViewport = Camera.Viewport;
                newViewport.X      = (int)p.X;
                newViewport.Y      = (int)p.Y;
                newViewport.Width  = (int) (p2.X - p.X);
                newViewport.Height = (int) (p2.Y - p.Y);
                Camera.UpdateAspectRatio(newViewport.AspectRatio);
                Camera.Viewport    = newViewport;
                Camera.UpdateProjectionMatrix();

                context.Viewport = Camera.Viewport;

                graphics.Begin();

                ModelExplorer.Render(context, renderArgs);
                
                graphics.End();
            }
        }
        
        public PlayerLocation EntityPosition { get; set; }
        
        class DebugModelRendererCamera : Camera
        {
            private readonly DebugModelRenderer _modelView;

            public Viewport Viewport { get; set; }
            public Vector3 EntityPositionOffset { get; set; } = new Vector3(0f, 0f, -6f);

            public DebugModelRendererCamera(DebugModelRenderer guiEntityModelView) : base(1 * 16 * 16)
            {
                _modelView = guiEntityModelView;
                Viewport = new Viewport(256, 128, 128, 256, 0f, 128.0f);
                Position = _modelView.EntityPosition;
                Rotation = Vector3.Zero;
                FOV = 25.0f;

                UpdateAspectRatio(Viewport.AspectRatio);
            }

            protected override void UpdateViewMatrix()
            {
                Matrix rotationMatrix = (Matrix.CreateRotationX(Rotation.X) *
                                        Matrix.CreateRotationY(Rotation.Y));

                Vector3 lookAtOffset = Vector3.Transform(EntityPositionOffset, rotationMatrix);

                Target = Position;

                Direction = Vector3.Transform(Vector3.Forward, rotationMatrix);

                ViewMatrix = Matrix.CreateLookAt(Target + lookAtOffset, Target, Vector3.Up);
            }

            public override void UpdateProjectionMatrix()
            {
                //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(Viewport.RenderBounds, NearDistance, FarDistance);
                ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Viewport.AspectRatio, NearDistance, Viewport.MaxDepth);
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