using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.Rendering.Camera;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics
{
    public class GuiPanoramaSkyBox
    {
        
        private bool CanRender { get; set; } = true;

        public Matrix WorldMatrix { get; set; } = Matrix.Identity; //Matrix.CreateScale(256f);

        public ICamera Camera {get; set; }

        private Model _skyBox;
        private TextureCube _skyBoxTexture;
        private Effect _skyBoxEffect;

        private Alex Game { get; }
        private GraphicsDevice GraphicsDevice { get; }
        private ContentManager Content { get; }
        

        public GuiPanoramaSkyBox(Alex alex, GraphicsDevice graphics, ContentManager content)
        {
            Game = alex;
            GraphicsDevice = graphics;
            Content = content;

            Camera = new FirstPersonCamera(1, Vector3.Zero, Vector3.Forward);
        }

        public void Load(IGuiRenderer renderer)
        {

            _skyBoxTexture = TextureUtils.TexturesToCube(GraphicsDevice, 256,
                                                         renderer.GetTexture2D(GuiTextures.Panorama0),
                                                         renderer.GetTexture2D(GuiTextures.Panorama1),
                                                         renderer.GetTexture2D(GuiTextures.Panorama2),
                                                         renderer.GetTexture2D(GuiTextures.Panorama3),
                                                         renderer.GetTexture2D(GuiTextures.Panorama4),
                                                         renderer.GetTexture2D(GuiTextures.Panorama5)
                                                        );

            _skyBox = Content.Load<Model>("CubeModel.xnb");
            _skyBoxEffect = Content.Load<Effect>("SkyboxEffect.xnb");

            CanRender = true;
        }


        private long _i;
        public void Update(GameTime gameTime)
        {
            var rotX = (float) Math.Sin(_i);
            var rotY = (float) Math.Cos(_i) / 2f;

            Camera.Rotation = new Vector3(rotX, rotY, 1f);
        }

        public void Draw(IRenderArgs args)
        {
            if (!CanRender) return;

            var g = args.GraphicsDevice;
            var camera = args.Camera;
            g.Clear(Color.SkyBlue);
            
            var depthState = g.DepthStencilState;
            var raster     = g.RasterizerState;
            var bl         = g.BlendState;

            g.DepthStencilState = new DepthStencilState() { DepthBufferEnable = false };
            g.RasterizerState   = new RasterizerState() { CullMode            = CullMode.None };
            g.BlendState        = BlendState.AlphaBlend;

            foreach (var pass in _skyBoxEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in _skyBox.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        part.Effect = _skyBoxEffect;
                        part.Effect.Parameters["World"].SetValue(WorldMatrix * Matrix.CreateTranslation(camera.Position));
                        part.Effect.Parameters["View"].SetValue(WorldMatrix * camera.ViewMatrix);
                        part.Effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(_skyBoxTexture);
                        part.Effect.Parameters["CameraPosition"].SetValue(camera.Position);
                    }

                    mesh.Draw();
                }
            }

            g.DepthStencilState = depthState;
            g.RasterizerState   = raster;
            g.BlendState        = bl;
        }
    }
}
