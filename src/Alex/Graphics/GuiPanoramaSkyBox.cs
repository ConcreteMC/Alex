using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Graphics.Models.Blocks;
using Alex.Rendering.Camera;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace Alex.Graphics
{
	public class GuiPanoramaSkyBox
    {
        private bool CanRender { get; set; } = false;

	    public Matrix World { get; set; } = Matrix.CreateRotationX(MathHelper.Pi);// * Matrix.CreateRotationZ(MathHelper.PiOver2);
	    public Matrix View { get; set; } = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
        public Matrix Projection { get; set; } = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(120.0f), 1.0f, 0.05f, 10.0f); //Matrix.CreateScale(256f);
		
        private AlphaTestEffect _skyBoxEffect;

        private Alex Game { get; }
        private GraphicsDevice GraphicsDevice { get; }
        private ContentManager Content { get; }

	    private VertexBuffer Buffer;
	    private IndexBuffer IndexBuffer;
		public GuiPanoramaSkyBox(Alex alex, GraphicsDevice graphics, ContentManager content)
        {
            Game = alex;
            GraphicsDevice = graphics;
            Content = content;
        }

	    public bool Loaded = false;

	    private Texture2D[] _textures;
        public void Load(IGuiRenderer renderer)
        {
			//_textures = new Texture2D[]
			//{
			//	renderer.GetTexture2D(GuiTextures.Panorama0),
			//	renderer.GetTexture2D(GuiTextures.Panorama2),

			//	renderer.GetTexture2D(GuiTextures.Panorama5), //Bottom
			//	renderer.GetTexture2D(GuiTextures.Panorama4), //Top

			//	renderer.GetTexture2D(GuiTextures.Panorama3), //Left
			//	renderer.GetTexture2D(GuiTextures.Panorama1)  //Right
			//};
	        _textures = new Texture2D[]
	        {
		        renderer.GetTexture2D(GuiTextures.Panorama0),
		        renderer.GetTexture2D(GuiTextures.Panorama1), //Right
		        renderer.GetTexture2D(GuiTextures.Panorama2),
		        renderer.GetTexture2D(GuiTextures.Panorama3), //Left
		        renderer.GetTexture2D(GuiTextures.Panorama4), //Top
		        renderer.GetTexture2D(GuiTextures.Panorama5), //Bottom

	        };
			
			CreateSkybox(GraphicsDevice);
			
            CanRender = true;
	        Loaded = true;
        }

	    private void CreateSkybox(GraphicsDevice device)
	    {
			_skyBoxEffect = new AlphaTestEffect(device);

		    _skyBoxEffect.World = World;
		    _skyBoxEffect.View = View;
			_skyBoxEffect.View = Matrix.Identity;
		    _skyBoxEffect.Projection = Projection;

			Buffer = new VertexBuffer(device, typeof(VertexPositionColorTexture), 64 * 6 * 4, BufferUsage.WriteOnly);
			IndexBuffer = new IndexBuffer(device, typeof(short), 64 * 6 * 6, BufferUsage.WriteOnly);
			UpdateSkyBoxCube();
	    }

	    private Matrix _rotationMatrix;
	    private void RotateSkyBox()
	    {
		    var xRot = (float) Math.Sin(_rotation / 400.0f) * 25.0f;// + 20.0f;

		    _rotationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(xRot))
		                  * Matrix.CreateRotationY(MathHelper.ToRadians(_rotation * 0.1f));
	    }

	    private void UpdateSkyBoxCube()
	    {
			short[] indexer = new short[64 * 6 * 6];
		    VertexPositionColorTexture[] data = new VertexPositionColorTexture[64 * 6 * 4];
			
		    for (int j = 0; j < 64; ++j)
		    {
			    var pos = new Vector3(((float)(j % 8) / 8.0f - 0.5f) / 64.0f, ((float)(j / 8) / 8.0f - 0.5f) / 64.0f, 0.0f);
				
			    var world = World;
			    world *= Matrix.CreateTranslation(pos);
			    //world *= _rotationMatrix;
				
			    for (int k = 0; k < 6; k++)
			    {
				    var indexerIndex = j * 6 + k * 6;
				    var dataIndex    = j * 6 + k * 4;
					
				    int alphaLevel = 255 / (j + 1);
				    var color      = new Color(255, 255, 255, alphaLevel);
					
				    var vm = Matrix.Identity;

				    if(k == 1) 
				    {
					    vm *= Matrix.CreateRotationY(MathHelper.PiOver2);
				    }
				    if (k == 2)
				    {
					    vm *= Matrix.CreateRotationY(MathHelper.Pi);
				    }
				    if (k == 3)
				    {
					    vm *= Matrix.CreateRotationY(-MathHelper.PiOver2);
				    }
				    if (k == 4)
				    {
					    vm *= Matrix.CreateRotationX(MathHelper.PiOver2);
				    }
				    if (k == 5)
				    {
					    vm *= Matrix.CreateRotationX(-MathHelper.PiOver2);
				    }

					data[dataIndex + 0].Position = Vector3.Transform(new Vector3(-1.0f, -1.0f, 1.0f), vm);
					data[dataIndex + 1].Position = Vector3.Transform(new Vector3(1.0f, -1.0f, 1.0f), vm);
					data[dataIndex + 2].Position = Vector3.Transform(new Vector3(1.0f, 1.0f, 1.0f), vm);
					data[dataIndex + 3].Position = Vector3.Transform(new Vector3(-1.0f, 1.0f, 1.0f), vm);

					//data[dataIndex + 0].RenderPosition = new Vector3(-1.0f, -1.0f, 1.0f);
					//data[dataIndex + 1].RenderPosition = new Vector3(1.0f, -1.0f, 1.0f);
					//data[dataIndex + 2].RenderPosition = new Vector3(1.0f, 1.0f, 1.0f);
					//data[dataIndex + 3].RenderPosition = new Vector3(-1.0f, 1.0f, 1.0f);

					data[dataIndex + 0].TextureCoordinate = new Vector2(0.0f, 0.0f);
				    data[dataIndex + 1].TextureCoordinate = new Vector2(1.0f, 0.0f);
				    data[dataIndex + 2].TextureCoordinate = new Vector2(1.0f, 1.0f);
				    data[dataIndex + 3].TextureCoordinate = new Vector2(0.0f, 1.0f);
					
				    data[dataIndex + 0].Color = color;
				    data[dataIndex + 1].Color = color;
				    data[dataIndex + 2].Color = color;
				    data[dataIndex + 3].Color = color;


				    indexer[indexerIndex + 0] = (short) (dataIndex + 0);
				    indexer[indexerIndex + 2] = (short) (dataIndex + 1);
				    indexer[indexerIndex + 1] = (short) (dataIndex + 2);
					
				    indexer[indexerIndex + 3] = (short) (dataIndex + 2);
				    indexer[indexerIndex + 5] = (short) (dataIndex + 3);
				    indexer[indexerIndex + 4] = (short) (dataIndex + 0);
			    }
			}
			
		    Buffer.SetData<VertexPositionColorTexture>(data);
		    IndexBuffer.SetData<short>(indexer);
	    }

	    private void DrawSkyBoxCube(GraphicsDevice graphics)
	    {
		    for (int j = 0; j < 64; ++j)
		    {
			    var pos = new Vector3(((float)(j % 8) / 8.0f - 0.5f) / 64.0f, ((float)(j / 8) / 8.0f - 0.5f) / 64.0f, 0.0f);
				
			    var world = World;
			    world *= Matrix.CreateTranslation(pos);
			    world *= _rotationMatrix;
			    _skyBoxEffect.World = world;

			    graphics.BlendFactor = new Color(255, 255, 255, 255);

			    for (int k = 0; k < 6; k++)
			    {
				    if (_textures[k] == null) continue;
					

				    var indexerIndex = j * 6 + k * 6;
				    var dataIndex    = j * 6 + k * 4;

				    _skyBoxEffect.VertexColorEnabled = true;
				    _skyBoxEffect.Texture = _textures[k];

				    _skyBoxEffect.Techniques[0].Passes[0].Apply();
				    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, indexerIndex, 2);
			    }
		    }
	    }



	    private float _rotation = 0f;
        public void Update(GameTime gameTime)
        {
	        _rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds / (1000.0f / 20.0f);

			//WorldMatrix = Matrix.CreateTranslation(-Vector3.One) * Matrix.CreateRotationY(_rotation) * Matrix.CreateTranslation(Vector3.One);
			
		// WorldMatrix = Matrix.CreateTranslation(-Vector3.One) 
	       //               * Matrix.CreateRotationY(_rotation) 
	       //               * Matrix.CreateTranslation(Vector3.One) 
						  //* Matrix.CreateTranslation(-pos);

	        //WorldMatrix = Matrix.CreateRotationX(MathHelper.Pi)
	        //            * Matrix.CreateRotationZ(MathHelper.PiOver2)
		       //         * Matrix.CreateRotationY(_rotation);

			//Camera.MoveTo(pos, rotation);

	        //_skyBoxEffect.World = WorldMatrix;
	        //_skyBoxEffect.View = Camera.ViewMatrix;
	        //_skyBoxEffect.Projection = Camera.ProjectionMatrix;

			RotateSkyBox();
        }

        public void Draw(IRenderArgs args)
        {
            if (!CanRender) return;

            var device = args.GraphicsDevice;

	        device.Clear(Color.SkyBlue);

			//g.Indices =
			var depthState = device.DepthStencilState;
	        var samplerState = device.SamplerStates[0];
            var rasterizerState = device.RasterizerState;
            var blendState = device.BlendState;
	        var viewport = device.Viewport;

	        var skyBoxBlendState = new BlendState()
	        {
		        ColorSourceBlend      = Blend.SourceAlpha,
		        ColorDestinationBlend = Blend.InverseSourceAlpha,
		        AlphaSourceBlend      = Blend.One,
		        AlphaDestinationBlend = Blend.Zero,

		        //ColorBlendFunction = BlendFunction.Add,
		        //AlphaBlendFunction = BlendFunction.Add,
		        IndependentBlendEnable = true,
		        //ColorWriteChannels = ColorWriteChannels.All
	        };
	        

			//device.Viewport = new Viewport(0, 0, 256, 256);
			device.DepthStencilState = DepthStencilState.None;
	        device.SamplerStates[0] = SamplerState.LinearClamp;
	        device.RasterizerState = RasterizerState.CullNone;
	        device.BlendState = skyBoxBlendState;

	        device.SetVertexBuffer(Buffer);
	        device.Indices = IndexBuffer;

	        _skyBoxEffect.CurrentTechnique.Passes[0].Apply();
			
	        DrawSkyBoxCube(device);



	        ////int x = 1;
	        //for (int x = 0; x < 6; x++)
	        //{
		       // if (_textures[x] == null) continue;

		       // _skyBoxEffect.Texture = _textures[x];
		       // _skyBoxEffect.Techniques[0].Passes[0].Apply();
		       // device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, x * 6, 2);
	        //}

			device.DepthStencilState = depthState;
	        device.SamplerStates[0]  = samplerState;
            device.RasterizerState   = rasterizerState;
            device.BlendState        = blendState;
	        device.Viewport          = viewport;
        }
    }
}
