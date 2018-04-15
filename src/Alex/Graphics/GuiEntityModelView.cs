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
        private Camera Camera { get; }
        private bool _canRender;
        
        public GuiEntityModelView(string entityName)
        {
            EntityName = entityName;
            EntityPosition = new PlayerLocation(Vector3.Zero);

            //Camera = new GuiEntityModelViewCamera(this);
            Camera = new ThirdPersonCamera(1, EntityPosition, Vector3.Zero);
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

                var g = args.Graphics;

                var blendState = g.BlendState;
                var depthStencilState = g.DepthStencilState;
                var rasterizerState = g.RasterizerState;
                var samplerState = g.SamplerStates[0];

                g.BlendState = BlendState.AlphaBlend;
                g.DepthStencilState = DepthStencilState.Default;
                g.RasterizerState = RasterizerState.CullClockwise;
                g.SamplerStates[0] = SamplerState.PointWrap;

                EntityModelRenderer?.Render(renderArgs, EntityPosition);

                g.BlendState = blendState;
                g.DepthStencilState = depthStencilState;
                g.RasterizerState = rasterizerState;
                g.SamplerStates[0] = samplerState;

                //args.Graphics.Viewport = viewport;
            }
        }
        
        class GuiEntityModelViewCamera : Camera
        {
            private GuiEntityModelView _modelView;

            public GuiEntityModelViewCamera(GuiEntityModelView guiEntityModelView) : base(1)
            {
                _modelView = guiEntityModelView;

                Position = Vector3.Zero;
                Rotation = Vector3.Zero;;
            }

            //public override void UpdateProjectionMatrix()
            //{
            //    var bounds = _modelView.Bounds;
            //    ProjectionMatrix =
            //        Matrix.CreatePerspectiveOffCenter(bounds.Left, bounds.Right, bounds.Bottom, bounds.Top,
            //                                          NearDistance, FarDistance);
            //}
            
            private Vector3 _thirdPersonOffset =  new Vector3(0, 2.5f, 3.5f);
            protected override void UpdateViewMatrix()
            {
                Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.X) *
                                        Matrix.CreateRotationY(Rotation.Y);

                Vector3 lookAtOffset = Vector3.Transform(_thirdPersonOffset, rotationMatrix);

                Target = Position;

                Direction = Vector3.Transform(Vector3.Forward, rotationMatrix);


                var heightOffset = new Vector3(0, 1.8f, 0);
                ViewMatrix = Matrix.CreateLookAt(Position + lookAtOffset, Target + heightOffset, Vector3.Up);
            }
        }
    }
}
