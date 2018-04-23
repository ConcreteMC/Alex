using System;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class GuiPanoramaSkyBox : ITexture2D
	{
		public Texture2D Texture => _renderTarget;
		public Rectangle ClipBounds => _renderTarget?.Bounds ?? new Rectangle(0, 0, 256, 256);
		public int Width { get; } = 256;
	    public int Height { get; } = 256;


        private bool CanRender { get; set; } = false;

	    public Matrix World { get; set; } = Matrix.CreateRotationX(MathHelper.Pi);// * Matrix.CreateRotationZ(MathHelper.PiOver2);
	    public Matrix View { get; set; } = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
        public Matrix Projection { get; set; } = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(120.0f), 1.0f, 0.05f, 10.0f); //Matrix.CreateScale(256f);
		
        private AlphaTestEffect _skyBoxEffect;
		
		
		private VertexBuffer Buffer;
		private IndexBuffer IndexBuffer;


	    //private RenderTarget2D _baseRenderTarget;
		private RenderTarget2D _renderTarget;

	    private BlendState _blendState;
	    private DepthStencilState _depthStencilState;
	    private SamplerState _samplerState;
	    private RasterizerState _rasterizerState;


	    public bool Loaded = false;

	    private Texture2D[] _textures;
		public GuiPanoramaSkyBox()
        {
	        
        }

        public void Load(IGuiRenderer renderer)
        {
	        _textures = new Texture2D[]
	        {
		        renderer.GetTexture2D(GuiTextures.Panorama0),
		        renderer.GetTexture2D(GuiTextures.Panorama1), //MaxX
		        renderer.GetTexture2D(GuiTextures.Panorama2),
		        renderer.GetTexture2D(GuiTextures.Panorama3), //MinX
		        renderer.GetTexture2D(GuiTextures.Panorama4), //MinY
		        renderer.GetTexture2D(GuiTextures.Panorama5), //MaxY
	        };

			
			CreateSkybox(Alex.Instance.GraphicsDevice);
			
            CanRender = true;
	        Loaded = true;
        }

	    private void CreateSkybox(GraphicsDevice device)
	    {
		    InitGraphics();
	        //_baseRenderTarget = new RenderTarget2D(GraphicsDevice, 256, 256, false, SurfaceFormat.Color, DepthFormat.None);
	        _renderTarget     = new RenderTarget2D(device, 256, 256, false, SurfaceFormat.Color, DepthFormat.None);

			_skyBoxEffect = new AlphaTestEffect(device);

		    _skyBoxEffect.World = World;
		    _skyBoxEffect.View = View;
			_skyBoxEffect.View = Matrix.Identity;
		    _skyBoxEffect.Projection = Projection;

		    _skyboxBuilder = new BufferBuilder<VertexPositionColorTexture>(device, 64 * 6);// 64 * 6
			
		    Buffer      = new VertexBuffer(device, typeof(VertexPositionColorTexture), 3 * 4, BufferUsage.WriteOnly);
		    IndexBuffer = new IndexBuffer(device, typeof(short), 3 * 6, BufferUsage.WriteOnly);

			UpdateSkyBoxCube();
		    UpdateRotateAndBlurSkyBox();
	    }

	    private Matrix _rotationMatrix;
	    private void RotateSkyBox()
	    {
		    var xRot = (float) Math.Sin(_rotation / 400.0f) * 25.0f;// + 20.0f;

		    _rotationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(xRot))
		                  * Matrix.CreateRotationY(MathHelper.ToRadians(_rotation * 0.1f));
	    }


		private BufferBuilder<VertexPositionColorTexture> _skyboxBuilder;
	    private void UpdateSkyBoxCube()
	    {

		    for (int j = 0; j < 64; ++j)
		    {
			    var pos = new Vector3(
			                          ((float)(j % 8) / 8.0f - 0.5f) / 64.0f, 
			                          ((float)(j / 8) / 8.0f - 0.5f) / 64.0f, 
			                          0.0f);
				
			    int alphaLevel = 255 / (j + 1);
			    var color      = new Color(255, 255, 255, alphaLevel);

			    for (int k = 0; k < 6; k++)
			    {
				    var i    = k + j * 6;
					
					
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

					_skyboxBuilder[i, 0].Position = Vector3.Transform(new Vector3(-1.0f, -1.0f, 1.0f), vm);
				    _skyboxBuilder[i, 1].Position = Vector3.Transform(new Vector3( 1.0f, -1.0f, 1.0f), vm);
				    _skyboxBuilder[i, 2].Position = Vector3.Transform(new Vector3( 1.0f,  1.0f, 1.0f), vm);
				    _skyboxBuilder[i, 3].Position = Vector3.Transform(new Vector3(-1.0f,  1.0f, 1.0f), vm);

				    _skyboxBuilder[i, 0].TextureCoordinate = new Vector2(0.0f, 0.0f);
				    _skyboxBuilder[i, 1].TextureCoordinate = new Vector2(1.0f, 0.0f);
				    _skyboxBuilder[i, 2].TextureCoordinate = new Vector2(1.0f, 1.0f);
				    _skyboxBuilder[i, 3].TextureCoordinate = new Vector2(0.0f, 1.0f);
					
				    _skyboxBuilder[i, 0].Color = color;
				    _skyboxBuilder[i, 1].Color = color;
				    _skyboxBuilder[i, 2].Color = color;
				    _skyboxBuilder[i, 3].Color = color;
			    }
			}

		    _skyboxBuilder.Build();
	    }

	    private void DrawSkyBoxCube(GraphicsDevice graphics)
	    {
		    for (int j = 0; j < 64; ++j)
		    {
			    var pos = new Vector3(((float)(j % 8) / 8.0f - 0.5f) / 64.0f, ((float)(j / 8) / 8.0f - 0.5f) / 64.0f, 0.0f); //x + InSizeX * (y + z * InSizeY)
				
			    var world = World;
			    world *= Matrix.CreateTranslation(pos);
			    world *= _rotationMatrix;
			    _skyBoxEffect.World = world;

			    graphics.BlendFactor = new Color(255, 255, 255, 255);

			    for (int k = 0; k < 6; k++)
			    {
				    if (_textures[k] == null) continue;
					
				    var indexerIndex = 6 * (k + j * 6);

				    _skyBoxEffect.VertexColorEnabled = true;
				    _skyBoxEffect.Texture = _textures[k];

				    _skyBoxEffect.Techniques[0].Passes[0].Apply();
				    graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, indexerIndex, 2);
			    }
		    }
	    }

		private void DrawSkyBoxBlur(GraphicsDevice graphics)
		{
			var world = World;
			_skyBoxEffect.World = world;

			for (int j = 0; j < 7; j++)
			for (int k = 0; k < 3; k++)
			{
				//if (_baseRenderTarget == null) continue;
				var indexerIndex = 6 * k;

				_skyBoxEffect.VertexColorEnabled = true;
				//_skyBoxEffect.Texture = _baseRenderTarget;

				_skyBoxEffect.Techniques[0].Passes[0].Apply();
				graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, indexerIndex, 2);
			}
		}


		private void UpdateRotateAndBlurSkyBox()
		{
			short[] indexer = new short[3 * 6];
			VertexPositionColorTexture[] data = new VertexPositionColorTexture[3 * 4];

			//var w = graphics.Viewport.Width;
			//var h = graphics.Viewport.Height;

			var w = 256;
			var h = 256;

			for (int k = 0; k < 3; ++k)
			{
				var indexerIndex = 6 * k;
				var dataIndex = 4 * k;

				float alpha = 1.0f / (float) (k + 1);
				float f1 = (float) (k - 1) / 256.0f;

				data[dataIndex + 0].Position = new Vector3(   w,    h,   0f);		data[dataIndex + 0].Color = new Color(Color.White, alpha);		data[dataIndex + 0].TextureCoordinate = new Vector2(0.0f + f1, 1.0f);
				data[dataIndex + 1].Position = new Vector3(   w, 0.0f,   0f);		data[dataIndex + 1].Color = new Color(Color.White, alpha);		data[dataIndex + 1].TextureCoordinate = new Vector2(1.0f + f1, 1.0f);
				data[dataIndex + 2].Position = new Vector3(0.0f, 0.0f,   0f);		data[dataIndex + 2].Color = new Color(Color.White, alpha);		data[dataIndex + 2].TextureCoordinate = new Vector2(1.0f + f1, 0.0f);
				data[dataIndex + 3].Position = new Vector3(0.0f,    h,   0f);		data[dataIndex + 3].Color = new Color(Color.White, alpha);		data[dataIndex + 3].TextureCoordinate = new Vector2(0.0f + f1, 0.0f);
				
				indexer[indexerIndex + 0] = (short) (dataIndex + 0); 
				indexer[indexerIndex + 2] = (short) (dataIndex + 1);
				indexer[indexerIndex + 1] = (short) (dataIndex + 2);
					
				indexer[indexerIndex + 3] = (short) (dataIndex + 2);
				indexer[indexerIndex + 5] = (short) (dataIndex + 3);
				indexer[indexerIndex + 4] = (short) (dataIndex + 0);

			}
			
			Buffer.SetData<VertexPositionColorTexture>(data);
			IndexBuffer.SetData<short>(indexer);
		}
		

	    private float _rotation = 0f;
        public void Update(GameTime gameTime)
        {
	        _rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds / (1000.0f / 20.0f);

			RotateSkyBox();
        }

	    private void InitGraphics()
	    {
		    _depthStencilState = DepthStencilState.None;
		    _rasterizerState   = RasterizerState.CullNone;

		    _blendState = new BlendState()
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
			
		    _samplerState = new SamplerState()
		    {
			    Filter = TextureFilter.Linear,
				FilterMode = TextureFilterMode.Comparison,
				ComparisonFunction = CompareFunction.Always,
			
			    //FilterMode    = TextureFilterMode.Default,
			    //Filter        = TextureFilter.Anisotropic,
			    MaxAnisotropy = 16,
			    AddressU      = TextureAddressMode.Clamp,
			    AddressV      = TextureAddressMode.Clamp,
			    AddressW      = TextureAddressMode.Clamp,
			    //ComparisonFunction = CompareFunction.
				
		    };
	    }

		private void RenderSkyBox(GraphicsContext context)
		{
			var device = context.GraphicsDevice;

			_skyboxBuilder.Bind();

			_skyBoxEffect.CurrentTechnique.Passes[0].Apply();
				
			DrawSkyBoxCube(device);
		}

		private void RotateAndBlurSkyBox(GraphicsContext context)
		{
			var device = context.GraphicsDevice;

			device.SetVertexBuffer(Buffer);
			device.Indices = IndexBuffer;

			_skyBoxEffect.CurrentTechnique.Passes[0].Apply();

			DrawSkyBoxBlur(device);
		}

		public void Draw(IRenderArgs args)
        {
            if (!CanRender) return;

            var device = args.GraphicsDevice;
			
	        device.Clear(Color.SkyBlue);

	        using (var context = GraphicsContext.CreateContext(args.GraphicsDevice, _blendState, _depthStencilState, _rasterizerState, _samplerState))
	        {
		        // Set the render target
		        //device.SetRenderTarget(_baseRenderTarget);
				
		        //RenderSkyBox(context);
				
		        // Set the render target
		        device.SetRenderTarget(_renderTarget);
				
		        RenderSkyBox(context);
		        //RotateAndBlurSkyBox(context);
				
		        // Drop the render target
		        device.SetRenderTarget(null);
	        }
        }


   //     public void Draw(IRenderArgs args)
   //     {
   //         if (!CanRender) return;

   //         var device = args.GraphicsDevice;

	  //      device.Clear(Color.SkyBlue);

			////g.Indices =
			//var depthState = device.DepthStencilState;
	  //      var samplerState = device.SamplerStates[0];
   //         var rasterizerState = device.RasterizerState;
   //         var blendState = device.BlendState;
	  //      var viewport = device.Viewport;

	  //      var skyBoxBlendState = new BlendState()
	  //      {
		 //       ColorSourceBlend      = Blend.SourceAlpha,
		 //       ColorDestinationBlend = Blend.InverseSourceAlpha,
		 //       AlphaSourceBlend      = Blend.One,
		 //       AlphaDestinationBlend = Blend.Zero,

		 //       //ColorBlendFunction = BlendFunction.Add,
		 //       //AlphaBlendFunction = BlendFunction.Add,
		 //       IndependentBlendEnable = true,
		 //       //ColorWriteChannels = ColorWriteChannels.All
	  //      };
	        

			////device.Viewport = new Viewport(0, 0, 256, 256);
			//device.DepthStencilState = DepthStencilState.None;
	  //      device.SamplerStates[0] = new SamplerState()
	  //      {
		 //       FilterMode = TextureFilterMode.Default,
		 //       Filter = TextureFilter.Anisotropic,
		 //       MaxAnisotropy = 16,
		 //       AddressU = TextureAddressMode.Clamp,
		 //       AddressV = TextureAddressMode.Clamp,
		 //       AddressW = TextureAddressMode.Clamp,
		 //       //ComparisonFunction = CompareFunction.

	  //      };
			//device.RasterizerState = RasterizerState.CullNone;
	  //      device.BlendState = skyBoxBlendState;

	  //      device.SetVertexBuffer(Buffer);
	  //      device.Indices = IndexBuffer;

	  //      _skyBoxEffect.CurrentTechnique.Passes[0].Apply();
			
	  //      DrawSkyBoxCube(device);



	  //      ////int x = 1;
	  //      //for (int x = 0; x < 6; x++)
	  //      //{
		 //      // if (_textures[x] == null) continue;

		 //      // _skyBoxEffect.Texture = _textures[x];
		 //      // _skyBoxEffect.Techniques[0].Passes[0].Apply();
		 //      // device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, x * 6, 2);
	  //      //}

			//device.DepthStencilState = depthState;
	  //      device.SamplerStates[0]  = samplerState;
   //         device.RasterizerState   = rasterizerState;
   //         device.BlendState        = blendState;
	  //      device.Viewport          = viewport;
   //     }
    }
}
