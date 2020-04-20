using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using MathF = System.MathF;

namespace Alex.Graphics.Models
{
	//Thanks https://github.com/SirCmpwn/TrueCraft
	public class SkyBox : IDisposable
	{
		private float MoonX = 1f/4f;
		private float MoonY = 1f/2f;

		private BasicEffect SkyPlaneEffect { get; set; }
	    private BasicEffect  CelestialPlaneEffect { get; set; }
		private BasicEffect CloudsPlaneEffect { get; set; }

	    private PooledVertexBuffer CloudsPlane { get; set; }
        private PooledVertexBuffer SkyPlane { get; set; }
	    private PooledVertexBuffer CelestialPlane { get; set; }
		private PooledVertexBuffer MoonPlane { get; }

		private PooledTexture2D SunTexture { get; }
		private PooledTexture2D MoonTexture { get; }
		private PooledTexture2D CloudTexture { get; }

		private bool CanRender { get; set; } = true;
		public bool EnableClouds { get; set; } = false;

		private World World { get; }

	    private readonly VertexPositionTexture[] _moonPlaneVertices;
	    private IOptionsProvider OptionsProvider { get; }
		private AlexOptions Options => OptionsProvider.AlexOptions;
		public SkyBox(IServiceProvider serviceProvider, GraphicsDevice device, World world)
		{
			World = world;
			//Game = alex;
			var alex = serviceProvider.GetRequiredService<Alex>();
			OptionsProvider = serviceProvider.GetRequiredService<IOptionsProvider>();

		    if (alex.Resources.ResourcePack.TryGetBitmap("environment/sun", out var sun))
		    {
			    SunTexture = TextureUtils.BitmapToTexture2D(device, sun);
		    }
		    else
		    {
			    CanRender = false;
			    return;
		    }

		    if (alex.Resources.ResourcePack.TryGetBitmap("environment/moon_phases", out var moonPhases))
		    {
			    MoonTexture = TextureUtils.BitmapToTexture2D(device, moonPhases);
		    }
		    else
		    {
			    CanRender = false;
			    return;
		    }

		    if (alex.Resources.ResourcePack.TryGetBitmap("environment/clouds", out var cloudTexture))
		    {
			    CloudTexture = TextureUtils.BitmapToTexture2D(device, cloudTexture);
			    EnableClouds = false;
		    }
		    else
		    {
			    EnableClouds = false;
		    }

            //var d = 144;

			CelestialPlaneEffect = new BasicEffect(device);
			CelestialPlaneEffect.VertexColorEnabled = false;
			CelestialPlaneEffect.LightingEnabled = false;
			CelestialPlaneEffect.TextureEnabled = true;

			SkyPlaneEffect = new BasicEffect(device);
			SkyPlaneEffect.VertexColorEnabled = false;
			SkyPlaneEffect.FogEnabled = true;
			SkyPlaneEffect.FogStart = 0;
			SkyPlaneEffect.FogEnd = 64 * 0.8f;
			SkyPlaneEffect.LightingEnabled = true;

			var planeDistance = 64;
			var plane = new[]
			{
				new VertexPositionColor(new Vector3(-planeDistance, 0, -planeDistance), Color.White),
				new VertexPositionColor(new Vector3(planeDistance, 0, -planeDistance), Color.White),
				new VertexPositionColor(new Vector3(-planeDistance, 0, planeDistance), Color.White),

				new VertexPositionColor(new Vector3(planeDistance, 0, -planeDistance), Color.White),
				new VertexPositionColor(new Vector3(planeDistance, 0, planeDistance), Color.White),
				new VertexPositionColor(new Vector3(-planeDistance, 0, planeDistance), Color.White)
			};
			SkyPlane = GpuResourceManager.GetBuffer(this, device, VertexPositionColor.VertexDeclaration,
				plane.Length, BufferUsage.WriteOnly);
			SkyPlane.SetData<VertexPositionColor>(plane);

			planeDistance = 60;
			var celestialPlane = new[]
			{
				new VertexPositionTexture(new Vector3(-planeDistance, 0, -planeDistance), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, 1)),

				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, planeDistance), new Vector2(1, 1)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, 1))
			};
			CelestialPlane = GpuResourceManager.GetBuffer(this, device, VertexPositionTexture.VertexDeclaration,
				celestialPlane.Length, BufferUsage.WriteOnly);
			CelestialPlane.SetData<VertexPositionTexture>(celestialPlane);

			_moonPlaneVertices = new[]
			{
				new VertexPositionTexture(new Vector3(-planeDistance, 0, -planeDistance), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(MoonX, 0)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, MoonY)),

				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(MoonX, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, planeDistance), new Vector2(MoonX, MoonY)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, MoonY)),
			};
			MoonPlane = GpuResourceManager.GetBuffer(this, device, VertexPositionTexture.VertexDeclaration,
				_moonPlaneVertices.Length, BufferUsage.WriteOnly);
			MoonPlane.SetData<VertexPositionTexture>(_moonPlaneVertices);

			if (EnableClouds)
				SetupClouds(device, planeDistance);

			RenderSkybox = Options.VideoOptions.Skybox.Value;
			Options.VideoOptions.Skybox.Bind(SkyboxSettingUpdated);
		}

		private void SkyboxSettingUpdated(bool oldvalue, bool newvalue)
		{
			RenderSkybox = newvalue;
		}

		private bool RenderSkybox { get; set; } = true;

		private void SetupClouds(GraphicsDevice device, int planeDistance)
		{
			CloudsPlaneEffect = new BasicEffect(device);
			CloudsPlaneEffect.Texture = CloudTexture;
			CloudsPlaneEffect.TextureEnabled = true;
			CloudsPlaneEffect.Alpha = 0.5f;
			
			var cloudVertices = new[]
			{
				new VertexPositionTexture(new Vector3(-planeDistance, 0, -planeDistance), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, 1)),

				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, planeDistance), new Vector2(1, 1)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, 1))
			};


            CloudsPlane = GpuResourceManager.GetBuffer(this, device, VertexPositionTexture.VertexDeclaration,
				cloudVertices.Length, BufferUsage.WriteOnly);
			CloudsPlane.SetData<VertexPositionTexture>(cloudVertices);
        }

		private float CelestialAngle
		{
			get
			{
				int i = (int)(World.WorldInfo.Time % 24000L);
				float f = ((float)i + 1f) / 24000.0F - 0.25F;

				if (f < 0.0F)
				{
					++f;
				}

				if (f > 1.0F)
				{
					--f;
				}

				float f1 = 1.0F - (float)((Math.Cos((double)f * Math.PI) + 1.0D) / 2.0D);
				f = f + (f1 - f) / 3.0F;
				return f;
			}
		}

		public float BrightnessModifier => MathHelper.Clamp(MathF.Cos(CelestialAngle * MathHelper.TwoPi) * 2 + 0.5f, 0.25f, 1f);

		public Color WorldSkyColor
	    {
		    get
		    {
			    var position = World.Camera.Position;

			    //float f1 = MathF.Cos(CelestialAngle * ((float)Math.PI * 2F)) * 2.0F + 0.5F;
			   // f1 = MathHelper.Clamp(f1, 0.0F, 1.0F);

			    int x = (int)MathF.Floor(position.X);
			    int y = (int)MathF.Floor(position.Y);
			    int z = (int)MathF.Floor(position.Z);

			    Biome biome = BiomeUtils.GetBiomeById(World.GetBiome(x, y, z));
			    float biomeTemperature = biome.Temperature;

			    biomeTemperature = biomeTemperature / 3.0F;
			    biomeTemperature = MathHelper.Clamp(biomeTemperature, -1.0F, 1.0F);
			    int l = MathUtils.HsvToRGB(0.62222224F - biomeTemperature * 0.05F, 0.5F + biomeTemperature * 0.1F, 1.0F);

			    float r = (l >> 16 & 255) / 255.0F;
			    float g = (l >> 8 & 255) / 255.0F;
			    float b = (l & 255) / 255.0F;
			    r = r * BrightnessModifier;
			    g = g * BrightnessModifier;
			    b = b * BrightnessModifier;

				return new Color(r,g,b);
		    }
	    }

	    public Color WorldFogColor
	    {
		    get
		    {
			    float f = MathF.Cos(CelestialAngle * ((float) Math.PI * 2F)) * 2.0F + 0.5F;
			    f = MathHelper.Clamp(f, 0.0F, 1.0F);

			    return new Color(0.7529412F * (f * 0.94F + 0.06F), 0.84705883F * (f * 0.94F + 0.06F),
				    1.0F * (f * 0.91F + 0.09F));
		    }
	    }

	    private float Blend(float source, float destination, float blendFactor)
	    {
		    return destination + (source - destination) * blendFactor;
	    }

	    private const float DrawDistance = 0f;
	    
	    public Color AtmosphereColor
		{
			get
			{
				float blendFactor = 0.29f;// 1.0f - System.MathF.Pow(1.0f - (4f - (DrawDistance)), 0.25f); //((Options.VideoOptions.RenderDistance ^2) / 100f) * 0.45f;
				
				var fog = WorldFogColor.ToVector3();
				var sky = WorldSkyColor.ToVector3();
				var color = new Vector3(Blend(sky.X, fog.X, blendFactor), Blend(sky.Y, fog.Y, blendFactor), Blend(sky.Z, fog.Z, blendFactor));
				// TODO: more stuff
				return new Color(color);
			}
		}

	    public Color VoidColor
	    {
		    get
		    {
			    WorldSkyColor.Deconstruct(out float r, out float g, out float b);
			    
			    return new Color(r * 0.2f + 0.04f, g * 0.2f + 0.04f, b * 0.6f + 0.1f);
		    }
	    }
	    
	    private int CurrentMoonPhase = 0;
	    public void Update(IUpdateArgs args)
	    {
		    if (!RenderSkybox) return;
		    
		    var moonPhase = (int)(World.WorldInfo.Time / 24000L % 8L + 8L) % 8;
		    if (CurrentMoonPhase != moonPhase)
		    {
			    CurrentMoonPhase = moonPhase;
			    
			    var w = (1f / MoonTexture.Width) * (MoonTexture.Width / 4f);
			    var h = (1f / MoonTexture.Height) * (MoonTexture.Height / 2f);

			    int x = moonPhase % 4;
			    int y = moonPhase % 2;

			    float textureX = (w * x);
			    float textureY = (h * y);

			    float textureXMax = (w * x) + w;
			    float textureYMax = (h * y) + h;

			    _moonPlaneVertices[0].TextureCoordinate = new Vector2(textureX, textureY);
			    _moonPlaneVertices[1].TextureCoordinate = new Vector2(textureXMax, textureY);
			    _moonPlaneVertices[2].TextureCoordinate = new Vector2(textureX, textureYMax);

			    _moonPlaneVertices[3].TextureCoordinate = new Vector2(textureXMax, textureY);
			    _moonPlaneVertices[4].TextureCoordinate = new Vector2(textureXMax, textureYMax);
			    _moonPlaneVertices[5].TextureCoordinate = new Vector2(textureX, textureYMax);

			    var modified = _moonPlaneVertices.Select(xx => xx.TextureCoordinate).ToArray();
			    MoonPlane.SetData(12, modified, 0, modified.Length, MoonPlane.VertexDeclaration.VertexStride);
		    }
		    
		    var camera = args.Camera;
		    SkyPlaneEffect.View = camera.ViewMatrix;
		    SkyPlaneEffect.Projection = camera.ProjectionMatrix;

		    CelestialPlaneEffect.View = camera.ViewMatrix;
		    CelestialPlaneEffect.Projection = camera.ProjectionMatrix;

		    var position = camera.Position;
		    
		    if (EnableClouds)
		    {
			    CloudsPlaneEffect.View = camera.ViewMatrix;
			    CloudsPlaneEffect.Projection = camera.ProjectionMatrix;
			    
			    CloudsPlaneEffect.World = Matrix.CreateTranslation(position.X, 127, position.Z);
		    }
	    }

	    private static DepthStencilState DepthStencilState { get; } = new DepthStencilState() { DepthBufferEnable = false };
	    private static RasterizerState RasterState { get; } = new RasterizerState() { CullMode = CullMode.None };
	    
	    private static BlendState CelestialBlendState { get; } = new BlendState()
	    {
		    AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
		    AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
		    ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
		    ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha
		    //  IndependentBlendEnable = true,
		    //AlphaBlendFunction = BlendFunction.Add,
		   // ColorBlendFunction = BlendFunction.Add
	    };
	    
	    public void Draw(IRenderArgs renderArgs)
	    {
		    renderArgs.GraphicsDevice.Clear(this.AtmosphereColor);
		    
		    if (!CanRender || !RenderSkybox) return;
		    
		    var depthState = renderArgs.GraphicsDevice.DepthStencilState;
		    var raster = renderArgs.GraphicsDevice.RasterizerState;
		    var bl = renderArgs.GraphicsDevice.BlendState;

		  //  renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState;
		  renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
		    renderArgs.GraphicsDevice.RasterizerState = RasterState;
			renderArgs.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			
			DrawSky(renderArgs, renderArgs.Camera.Position);

			var backup = renderArgs.GraphicsDevice.BlendState;
			
		    renderArgs.GraphicsDevice.BlendState = CelestialBlendState;

		    DrawSun(renderArgs, renderArgs.Camera.Position);

			DrawMoon(renderArgs, renderArgs.Camera.Position);

			renderArgs.GraphicsDevice.BlendState = backup;

			DrawVoid(renderArgs, renderArgs.Camera.Position);

		    renderArgs.GraphicsDevice.DepthStencilState = depthState;
		    renderArgs.GraphicsDevice.RasterizerState = raster;
		    renderArgs.GraphicsDevice.BlendState = bl;
	    }

		private void DrawSky(IRenderArgs renderArgs, Vector3 position)
		{
			SkyPlaneEffect.FogColor = AtmosphereColor.ToVector3();
			SkyPlaneEffect.World = Matrix.CreateRotationX(MathHelper.Pi)
			                       * Matrix.CreateTranslation(0, 16, 0)
			                       * Matrix.CreateTranslation(position);
			                    //   * Matrix.CreateTranslation(position);
		    
			SkyPlaneEffect.AmbientLightColor = WorldSkyColor.ToVector3();
			
			// Sky
			renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);
			foreach (var pass in SkyPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, SkyPlane.VertexCount);
			}
		}

		private void DrawClouds(IRenderArgs renderArgs, Vector3 position)
		{
			// Clouds
			//CloudsPlaneEffect.AmbientLightColor = WorldSkyColor.ToVector3();

            renderArgs.GraphicsDevice.SetVertexBuffer(CloudsPlane);
			foreach (var pass in CloudsPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
			}
		}

        private void DrawSun(IRenderArgs renderArgs, Vector3 position)
		{
			// Sun
			CelestialPlaneEffect.Texture = SunTexture;
			CelestialPlaneEffect.World =
				Matrix.CreateTranslation(0, 100, 0)
			                             * Matrix.CreateRotationX(MathHelper.TwoPi * CelestialAngle) * Matrix.CreateTranslation(position);

			renderArgs.GraphicsDevice.SetVertexBuffer(CelestialPlane);
			foreach (var pass in CelestialPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, CelestialPlane.VertexCount);
			}
			
		}

		private void DrawMoon(IRenderArgs renderArgs, Vector3 position)
		{
			// Moon
			CelestialPlaneEffect.Texture = MoonTexture;
			CelestialPlaneEffect.World = Matrix.CreateTranslation(0, -100, 0)
			                                                                                     * Matrix.CreateRotationX(MathHelper.TwoPi * CelestialAngle) * Matrix.CreateTranslation(position);

			renderArgs.GraphicsDevice.SetVertexBuffer(MoonPlane);
			foreach (var pass in CelestialPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, MoonPlane.VertexCount);
			}
			
		}

		private void DrawVoid(IRenderArgs renderArgs, Vector3 position)
		{
			// Void
			renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);

			SkyPlaneEffect.World = Matrix.CreateTranslation(0, -16, 0) * Matrix.CreateTranslation(position);
			SkyPlaneEffect.AmbientLightColor = VoidColor.ToVector3();

			foreach (var pass in SkyPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, SkyPlane.VertexCount);
			}
		}

		public void Dispose()
		{
			CloudsPlane?.MarkForDisposal();
			SkyPlane?.MarkForDisposal();
			CelestialPlane?.MarkForDisposal();
			MoonPlane?.MarkForDisposal();
			SunTexture?.MarkForDisposal();
			MoonTexture?.MarkForDisposal();
			CloudTexture?.MarkForDisposal();
			
			SkyPlaneEffect?.Dispose();
			CelestialPlaneEffect?.Dispose();
			CloudsPlaneEffect?.Dispose();
		}
		
		private static Color HSL2RGB(float h, float sl, float l)
		{
			// Thanks http://www.java2s.com/Code/CSharp/2D-Graphics/HSLtoRGBconversion.htm
			float v, r, g, b;
			r = g = b = l;   // default to gray
			v = (l <= 0.5f) ? (l * (1.0f + sl)) : (l + sl - l * sl);
			if (v > 0)
			{
				int sextant;
				float m, sv, fract, vsf, mid1, mid2;
				m = l + l - v;
				sv = (v - m) / v;
				h *= 6.0f;
				sextant = (int)h;
				fract = h - sextant;
				vsf = v * sv * fract;
				mid1 = m + vsf;
				mid2 = v - vsf;
				switch (sextant)
				{
					case 0:
						r = v; g = mid1; b = m;
						break;
					case 1:
						r = mid2; g = v; b = m;
						break;
					case 2:
						r = m; g = v; b = mid1;
						break;
					case 3:
						r = m; g = mid2; b = v;
						break;
					case 4:
						r = mid1; g = m; b = v;
						break;
					case 5:
						r = v; g = m; b = mid2;
						break;
				}
			}
			return new Color(r, g, b);
		}
	}
}
