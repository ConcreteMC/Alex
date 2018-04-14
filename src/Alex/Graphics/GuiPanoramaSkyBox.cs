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

namespace Alex.Graphics
{
	public class GuiPanoramaSkyBox
    {
        private bool CanRender { get; set; } = false;

        public Matrix WorldMatrix { get; set; } = Matrix.Identity; //Matrix.CreateScale(256f);

        public  FirstPersonCamera Camera {get; set; }

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

            Camera = new FirstPersonCamera(12, new Vector3(1, 1, 1), Vector3.Forward);
        }

	    public bool Loaded = false;

	    private Texture2D[] _textures;
        public void Load(IGuiRenderer renderer)
        {
			_textures = new Texture2D[]
			{
				renderer.GetTexture2D(GuiTextures.Panorama0),
				renderer.GetTexture2D(GuiTextures.Panorama2),

				renderer.GetTexture2D(GuiTextures.Panorama5), //Bottom
				renderer.GetTexture2D(GuiTextures.Panorama4), //Top

				renderer.GetTexture2D(GuiTextures.Panorama1), //Left
				renderer.GetTexture2D(GuiTextures.Panorama3)  //Right
			};
			
			CreateSkybox(GraphicsDevice);
			
            CanRender = true;
	        Loaded = true;
        }

		public void CreateSkybox(GraphicsDevice device)
		{
			_skyBoxEffect = new AlphaTestEffect(device);

			Buffer = new VertexBuffer(device,
								typeof(VertexPositionTexture),
								4 * 6,
								BufferUsage.WriteOnly);

			VertexPositionTexture[] data = new VertexPositionTexture[4 * 6];

			float y = 0.3f;

			short[] indexer = new short[6 * 6];

			#region Define Vertexes
			Vector3 vExtents = new Vector3(256, 256, 256);
			//back
			data[0].Position = new Vector3(vExtents.X, -vExtents.Y * y, -vExtents.Z);
			data[0].TextureCoordinate.X = 0f;
			data[0].TextureCoordinate.Y = 1.0f;

			data[1].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
			data[1].TextureCoordinate.X = 0.0f;
			data[1].TextureCoordinate.Y = 0.0f;


			data[2].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
			data[2].TextureCoordinate.X = 1f;
			data[2].TextureCoordinate.Y = 0.0f;

			data[3].Position = new Vector3(-vExtents.X, -vExtents.Y * y, -vExtents.Z);
			data[3].TextureCoordinate.X = 1f;
			data[3].TextureCoordinate.Y = 1.0f;

			//front
			data[4].Position = new Vector3(-vExtents.X, -vExtents.Y * y, vExtents.Z);
			data[4].TextureCoordinate.X = 1f;
			data[4].TextureCoordinate.Y = 1.0f;

			data[5].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
			data[5].TextureCoordinate.X = 1f;
			data[5].TextureCoordinate.Y = 0.0f;

			data[6].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
			data[6].TextureCoordinate.X = 0.0f;
			data[6].TextureCoordinate.Y = 0.0f;

			data[7].Position = new Vector3(vExtents.X, -vExtents.Y * y, vExtents.Z);
			data[7].TextureCoordinate.X = 0.0f;
			data[7].TextureCoordinate.Y = 1.0f;

			//bottom (2)
			data[8].Position = new Vector3(-vExtents.X, -vExtents.Y * y, -vExtents.Z);
			data[8].TextureCoordinate.X = 1.0f;
			data[8].TextureCoordinate.Y = 0.0f;

			data[9].Position = new Vector3(-vExtents.X, -vExtents.Y * y, vExtents.Z);
			data[9].TextureCoordinate.X = 1.0f;
			data[9].TextureCoordinate.Y = 1f;

			data[10].Position = new Vector3(vExtents.X, -vExtents.Y * y, vExtents.Z);
			data[10].TextureCoordinate.X = 0.0f;
			data[10].TextureCoordinate.Y = 1f;

			data[11].Position = new Vector3(vExtents.X, -vExtents.Y * y, -vExtents.Z);
			data[11].TextureCoordinate.X = 0.0f;
			data[11].TextureCoordinate.Y = 0.0f;

			//top (3)
			data[12].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
			data[12].TextureCoordinate.X = 0.5f;
			data[12].TextureCoordinate.Y = 0.0f;

			data[13].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
			data[13].TextureCoordinate.X = 0.5f;
			data[13].TextureCoordinate.Y = 1.0f;

			data[14].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
			data[14].TextureCoordinate.X = 1.0f;
			data[14].TextureCoordinate.Y = 1.0f;

			data[15].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
			data[15].TextureCoordinate.X = 1.0f;
			data[15].TextureCoordinate.Y = 0.0f;

			//left
			data[16].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
			data[16].TextureCoordinate.X = 1f;
			data[16].TextureCoordinate.Y = 1.0f;

			data[17].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
			data[17].TextureCoordinate.X = 0f;
			data[17].TextureCoordinate.Y = 1.0f;

			data[18].Position = new Vector3(-vExtents.X, -vExtents.Y * y, vExtents.Z);
			data[18].TextureCoordinate.X = 0f;
			data[18].TextureCoordinate.Y = 0.0f;

			data[19].Position = new Vector3(-vExtents.X, -vExtents.Y * y, -vExtents.Z);
			data[19].TextureCoordinate.X = 1.0f;
			data[19].TextureCoordinate.Y = 0.0f;

			//right
			data[20].Position = new Vector3(vExtents.X, -vExtents.Y * y, -vExtents.Z);
			data[20].TextureCoordinate.X = 0.0f;
			data[20].TextureCoordinate.Y = 1.0f;

			data[21].Position = new Vector3(vExtents.X, -vExtents.Y * y, vExtents.Z);
			data[21].TextureCoordinate.X = 1.0f;
			data[21].TextureCoordinate.Y = 0.0f;

			data[22].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
			data[22].TextureCoordinate.X = 1.0f;
			data[22].TextureCoordinate.Y = 0.0f;

			data[23].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
			data[23].TextureCoordinate.X = 0.0f;
			data[23].TextureCoordinate.Y = 0.5f;

			Buffer.SetData<VertexPositionTexture>(data);


			IndexBuffer = new IndexBuffer(device,
								typeof(short), 6 * 6,
								BufferUsage.WriteOnly);

			for (int x = 0; x < 6; x++)
			{
				indexer[x * 6 + 0] = (short)(x * 4 + 0);
				indexer[x * 6 + 2] = (short)(x * 4 + 1);
				indexer[x * 6 + 1] = (short)(x * 4 + 2);

				indexer[x * 6 + 3] = (short)(x * 4 + 2);
				indexer[x * 6 + 5] = (short)(x * 4 + 3);
				indexer[x * 6 + 4] = (short)(x * 4 + 0);
			}

			IndexBuffer.SetData<short>(indexer);
			#endregion

		}


	    private float _rotation = 0f;
        public void Update(GameTime gameTime)
        {
	        _rotation += 0.2f * 
	                     (float)gameTime.ElapsedGameTime.TotalSeconds;

			WorldMatrix = Matrix.CreateTranslation(-Vector3.One) * Matrix.CreateRotationY(_rotation) * Matrix.CreateTranslation(Vector3.One);
	        _skyBoxEffect.World = WorldMatrix;
	        _skyBoxEffect.View = Camera.ViewMatrix;
	        _skyBoxEffect.Projection = Camera.ProjectionMatrix;
        }

        public void Draw(IRenderArgs args)
        {
            if (!CanRender) return;

            var device = args.GraphicsDevice;

	        device.Clear(Color.SkyBlue);

			//g.Indices =
			var depthState = device.DepthStencilState;
            var raster     = device.RasterizerState;
            var bl         = device.BlendState;

			device.DepthStencilState = DepthStencilState.None;
	        device.SamplerStates[0] = SamplerState.LinearWrap;
	        device.RasterizerState = RasterizerState.CullCounterClockwise;
	        device.SetVertexBuffer(Buffer);
	        device.Indices = IndexBuffer;
	        _skyBoxEffect.CurrentTechnique.Passes[0].Apply();

	        //int x = 1;
	        for (int x = 0; x < 6; x++)
	        {
		        if (_textures[x] == null) continue;

		        _skyBoxEffect.Texture = _textures[x];
		        _skyBoxEffect.Techniques[0].Passes[0].Apply();
		        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
			        0, 0, Buffer.VertexCount, x * 6, 2);
	        }

			device.DepthStencilState = depthState;
            device.RasterizerState   = raster;
            device.BlendState        = bl;
        }
    }
}
