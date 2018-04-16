using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Graphics.Models.Entity;
using Alex.Rendering.Camera;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics
{
    public class GuiEntityModelView : GuiElement
    {
        public PlayerLocation EntityPosition { get; set; }

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

        private Texture2D _skinTexture;

        public Texture2D SkinTexture
        {
            get => _skinTexture;
            set
            {
                _skinTexture = value;
                InitRenderer();
            }
        }
        
        private EntityModelRenderer EntityModelRenderer { get; set; }
        private GuiEntityModelViewCamera Camera { get; }
        private bool _canRender;
        
        public GuiEntityModelView(string entityName)
        {
            EntityName = entityName;
            EntityPosition = new PlayerLocation(Vector3.Zero);
            DefaultBackgroundTexture = GuiTextures.PanelGeneric;

            //Camera = new GuiEntityModelViewCamera(this);
            Camera = new GuiEntityModelViewCamera(this);
        }
        

        public void SetEntityRotation(float yaw, float pitch)
        {
            EntityPosition.Yaw     = yaw;
            EntityPosition.Pitch   = pitch;
        }

        public void SetEntityRotation(float yaw, float pitch, float headYaw)
        {
            EntityPosition.Yaw = yaw;
            EntityPosition.Pitch = pitch;
            EntityPosition.HeadYaw = headYaw;
        }

        private void InitRenderer()
        {
            if (string.IsNullOrWhiteSpace(EntityName) || SkinTexture == null)
            {
                _canRender = false;
                EntityModelRenderer = null;
                return;
            }
            Alex.Instance.Resources.BedrockResourcePack.EntityModels.TryGetValue(EntityName, out EntityModel m);

            EntityModelRenderer = new EntityModelRenderer(m, SkinTexture);
            _canRender = true;
        }
        
        private Rectangle _previousBounds;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_canRender)
            {
                var bounds = Screen.Bounds;

                if (bounds != _previousBounds)
                {
                    //var c = bounds.Center;
                    //Camera.Position = new Vector3(c.X, c.Y, 0.0f);
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

                Camera.MoveTo(EntityPosition, Vector3.Zero);
                
                EntityModelRenderer?.Update(updateArgs, EntityPosition);
            }
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            base.OnDraw(args);

            if (_canRender)
            {
                var renderArgs = new RenderArgs()
                {
                    GraphicsDevice = args.Graphics,
                    SpriteBatch    = args.SpriteBatch,
                    GameTime       = args.GameTime,
                    Camera         = Camera,
                };

                //var viewport = args.Graphics.Viewport;
                //args.Graphics.Viewport = new Viewport(Bounds);

                args.EndSpriteBatch();

                var g = args.Graphics;

                var blendState = g.BlendState;
                var depthStencilState = g.DepthStencilState;
                var rasterizerState = g.RasterizerState;
                var samplerState = g.SamplerStates[0];
                var viewport = g.Viewport;
                var scissor = g.ScissorRectangle;

                g.BlendState = BlendState.AlphaBlend;
                g.DepthStencilState = DepthStencilState.Default;
                g.RasterizerState = RasterizerState.CullClockwise;
                g.SamplerStates[0] = SamplerState.PointWrap;
                var newViewport = Camera.Viewport;
                var bounds = Bounds;
                
                bounds.Inflate(-3, -3);

                var p = Vector2.Transform(bounds.Location.ToVector2(), args.ScaledResolution.TransformMatrix);
                var p2 = Vector2.Transform(bounds.Location.ToVector2() + bounds.Size.ToVector2(), args.ScaledResolution.TransformMatrix);

                newViewport.X = (int)p.X;
                newViewport.Y = (int)p.Y;
                newViewport.Width = (int) (p2.X - p.X);
                newViewport.Height = (int) (p2.Y - p.Y);
                Camera.Viewport = newViewport;
                Camera.UpdateProjectionMatrix();

                //g.Viewport = newViewport;
                g.Viewport = newViewport;
                //g.ScissorRectangle = Bounds;
                
                //args.BeginSpriteBatch();

                EntityModelRenderer?.Render(renderArgs, EntityPosition);
                
                //args.EndSpriteBatch();

                g.BlendState = blendState;
                g.DepthStencilState = depthStencilState;
                g.RasterizerState = rasterizerState;
                g.SamplerStates[0] = samplerState;
                g.Viewport = viewport;
                //g.ScissorRectangle = scissor;
                
                args.BeginSpriteBatch();
                //args.Graphics.Viewport = viewport;
            }
        }
        
        class GuiEntityModelViewCamera : Camera
        {
            private readonly GuiEntityModelView _modelView;

            public Viewport Viewport { get; set; }
            public Vector3 EntityPositionOffset { get; set; } = new Vector3(0f, 0f, -6f);

            public GuiEntityModelViewCamera(GuiEntityModelView guiEntityModelView) : base(1)
            {
                _modelView = guiEntityModelView;
                Viewport = new Viewport(256, 128, 128, 256, 0.01f, 16.0f);
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

                ViewMatrix = Matrix.CreateLookAt(Target + lookAtOffset, Target + (Vector3.Up * 1.8f), Vector3.Up);
            }

            public override void UpdateProjectionMatrix()
            {
                //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(Viewport.Bounds, NearDistance, FarDistance);
                ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Viewport.AspectRatio, NearDistance, FarDistance);
            }

            //public override void UpdateProjectionMatrix()
            //{
            //    var bounds = _modelView.Bounds;
            //    ProjectionMatrix =
            //        Matrix.CreatePerspective(Viewport.Width, Viewport.Height, Viewport.MinDepth, Viewport.MaxDepth);
            //    //ProjectionMatrix = Matrix.CreateOrthographicOffCenter(Viewport.Bounds, NearDistance, FarDistance);// * Matrix.CreateTranslation(new Vector3(bounds.Location.ToVector2(), -1f));
            //    //ProjectionMatrix = Matrix.CreatePerspectiveOffCenter(_modelView.Bounds, NearDistance, FarDistance);
            //}
        }
    }
}
