using System;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class GuiPanoramaSkyBox : ITexture2D
	{
		public Texture2D Texture => _renderTarget;
		public Rectangle ClipBounds => _renderTarget?.Bounds ?? new Rectangle(0, 0, Width, Height);
		public int Width { get; } = 256;
	    public int Height { get; } = 256;


        private bool CanRender { get; set; } = false;
		
        private AlphaTestEffect _skyBoxEffect;
		private RenderTarget2D _renderTarget;

	    private BlendState _blendState;
	    private DepthStencilState _depthStencilState;
	    private SamplerState _samplerState;
	    private RasterizerState _rasterizerState;


	    public bool Loaded = false;

	    private Texture2D[] _textures;

		private Alex Game { get; }
		public GuiPanoramaSkyBox(Alex alex)
		{
			Game = alex;
		}

        public void Load(IGuiRenderer renderer)
        {
	        _textures = new Texture2D[]
	        {
		        renderer.GetTexture2D(GuiTextures.Panorama0),
		        renderer.GetTexture2D(GuiTextures.Panorama1),
		        renderer.GetTexture2D(GuiTextures.Panorama2),
		        renderer.GetTexture2D(GuiTextures.Panorama3),
		        renderer.GetTexture2D(GuiTextures.Panorama4),
		        renderer.GetTexture2D(GuiTextures.Panorama5),
	        };

			CreateSkybox(Game.GraphicsDevice);
			
            CanRender = true;
	        Loaded = true;
        }

		private void CreateSkybox(GraphicsDevice device)
	    {
		    InitGraphics();
	        _renderTarget     = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);

		    _skyBoxEffect = new AlphaTestEffect(device)
		    {
			    View = Matrix.Identity,
			    Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(120.0f), 1.0f, 0.05f, 10.0f)
		    };

		    _skyboxBuilder = new BufferBuilder<VertexPositionColorTexture>(device, 64 * 6);

			UpdateSkyBoxCube();
	    }

	    private Matrix _rotationMatrix;
	    private void RotateSkyBox()
	    {
		    var xRot = (float) Math.Sin(_rotation / 400.0f) * 25.0f;

		    _rotationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(xRot))
		                  * Matrix.CreateRotationY(MathHelper.ToRadians(_rotation * 0.1f));
	    }


		private BufferBuilder<VertexPositionColorTexture> _skyboxBuilder;
	    private void UpdateSkyBoxCube()
	    {
		    for (int j = 0; j < 64; ++j)
		    {
			    int alphaLevel = 255 / (j + 1);
			    var color      = new Color(255, 255, 255, alphaLevel);

			    for (int face = 0; face < 6; face++)
			    {
				    var i    = face + j * 6;
					
				    var vm = Matrix.Identity;

				    switch (face)
				    {
					    case 1:
						    vm *= Matrix.CreateRotationY(MathHelper.PiOver2);
						    break;
					    case 2:
						    vm *= Matrix.CreateRotationY(MathHelper.Pi);
						    break;
					    case 3:
						    vm *= Matrix.CreateRotationY(-MathHelper.PiOver2);
						    break;
					    case 4:
						    vm *= Matrix.CreateRotationX(MathHelper.PiOver2);
						    break;
					    case 5:
						    vm *= Matrix.CreateRotationX(-MathHelper.PiOver2);
						    break;
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
				
			    var world = Matrix.CreateRotationX(MathHelper.Pi);
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

	    private float _rotation = 0f;
        public void Update(GameTime gameTime)
        {
	        _rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds / (1000.0f / 20.0f);

			RotateSkyBox();
        }

		private void InitGraphics()
		{
			_depthStencilState = DepthStencilState.None;
			_rasterizerState = RasterizerState.CullNone;

			_blendState = new BlendState()
			{
				ColorSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaSourceBlend = Blend.One,
				AlphaDestinationBlend = Blend.Zero,

				IndependentBlendEnable = false,
			};

			_samplerState = new SamplerState()
			{
				Filter = TextureFilter.Anisotropic,
				FilterMode = TextureFilterMode.Comparison,
				ComparisonFunction = CompareFunction.Always,

				MaxAnisotropy = 16,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
			};
		}

		public void Draw(IRenderArgs args)
        {
            if (!CanRender) return;

            var device = args.GraphicsDevice;
			
	        device.Clear(Color.SkyBlue);

	        using (var context = GraphicsContext.CreateContext(args.GraphicsDevice, _blendState, _depthStencilState, _rasterizerState, _samplerState))
	        {
		        device.SetRenderTarget(_renderTarget);

		        _skyboxBuilder.Bind();
				DrawSkyBoxCube(device);

		        device.SetRenderTarget(null);
	        }
        }
    }
}
