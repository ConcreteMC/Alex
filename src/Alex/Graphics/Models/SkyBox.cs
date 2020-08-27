using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Graphics.Effect;
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

		private SkyEffect   SkyPlaneEffect       { get; set; }
	    private BasicEffect CelestialPlaneEffect { get; set; }
		private BasicEffect CloudsPlaneEffect    { get; set; }

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
		    
		    var planeDistance = 64;
            //var d = 144;

			CelestialPlaneEffect = new BasicEffect(device);
			CelestialPlaneEffect.VertexColorEnabled = false;
			CelestialPlaneEffect.LightingEnabled = false;
			CelestialPlaneEffect.TextureEnabled = true;

			SkyPlaneEffect = new SkyEffect(device);
			//SkyPlaneEffect.
		//	SkyPlaneEffect.VertexColorEnabled = true;
			SkyPlaneEffect.FogEnabled = true;
			SkyPlaneEffect.FogStart = 0;
			SkyPlaneEffect.FogEnd = (planeDistance / 10f) * 8f;
		//	SkyPlaneEffect.LightingEnabled = false;
			//SkyPlaneEffect.AmbientLightColor
			//SkyPlaneEffect.DiffuseColor = Color.White.ToVector3();
			
			var plane = new[]
			{
				new VertexPosition(new Vector3(-planeDistance, 0, -planeDistance)),
				new VertexPosition(new Vector3(planeDistance, 0, -planeDistance)),
				new VertexPosition(new Vector3(-planeDistance, 0, planeDistance)),

				new VertexPosition(new Vector3(planeDistance, 0, -planeDistance)),
				new VertexPosition(new Vector3(planeDistance, 0, planeDistance)),
				new VertexPosition(new Vector3(-planeDistance, 0, planeDistance))
			};
			SkyPlane = GpuResourceManager.GetBuffer(this, device, VertexPosition.VertexDeclaration,
				plane.Length, BufferUsage.WriteOnly);
			SkyPlane.SetData<VertexPosition>(plane);

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
				var x = ((float)World.Time / 24000) - 0.25f;
				if (x < 0) x+= 1;

				return x + ((1f - (System.MathF.Cos(x * MathF.PI) + 1f) / 2f) - x) / 3f;
				/*int i = (int)(World.Time % 24000L);
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
				return f;*/
			}
		}

		public float BrightnessModifier => MathHelper.Clamp(MathF.Cos(CelestialAngle * MathHelper.TwoPi) * 2 + 0.5f, 0f, 1f);

		private Color BaseColor
		{
			get
			{
				// Note: temperature comes from the current biome, but we have to
				// do biomes differently than Minecraft so we'll un-hardcode this later.
				const float temp = 0.8f / 3;
				return HSL2RGB(0.6222222f - temp * 0.05f, 0.5f + temp * 0.1f, BrightnessModifier);
			}
		}
		
		public Color WorldSkyColor
	    {
		    get
		    {
			    return BaseColor;
			    var position = new BlockCoordinates( World.Camera.Position);

			    //float f1 = MathF.Cos(CelestialAngle * ((float)Math.PI * 2F)) * 2.0F + 0.5F;
			   // f1 = MathHelper.Clamp(f1, 0.0F, 1.0F);
			   
			    Biome biome = World.GetBiome(position);
			    float biomeTemperature = biome.Temperature;

			    biomeTemperature = biomeTemperature / 3.0F;
			    biomeTemperature = MathHelper.Clamp(biomeTemperature, -1.0F, 1.0F);
			  //  var   brightness       = cos(CelestialAngle * MathHelper.TwoPi) *2 + 0.5;
			    
			   // float biomeTemperature = 0f;
			    return MathUtils.HslToRGB(0.62222224F - biomeTemperature * 0.05F, 0.5F + biomeTemperature * 0.1F, 1.0F) * BrightnessModifier;
/*
			    float r = (l >> 16 & 255) / 255.0F;
			    float g = (l >> 8 & 255) / 255.0F;
			    float b = (l & 255) / 255.0F;
			    r = r * BrightnessModifier;
			    g = g * BrightnessModifier;
			    b = b * BrightnessModifier;

				return new Color(r,g,b);*/
		    }
	    }

	    public Color WorldFogColor
	    {
		    get
		    {
			    if (World.Dimension == Dimension.Nether)
			    {
				    return new Color(0.2f, 0.03f, 0.03f);
			    }

			    float y = BrightnessModifier;
			    return new Color(0.7529412f * y * 0.94f + 0.06f,
				    0.8470588f * y * 0.94f + 0.06f, 1.0f * y * 0.91f + 0.09f);
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
				//In Water: 0.02R, 0.02G, 0.2B
				//In Lava: 0.6 0.1 0.0
				
				float blendFactor = 0.29f;// 1.0f - System.MathF.Pow(1.0f - (4f - (DrawDistance)), 0.25f); //((Options.VideoOptions.RenderDistance ^2) / 100f) * 0.45f;
				
			//	WorldSkyColor.Deconstruct(out float skyR, out float g, out float b);
				
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
		    
		    var moonPhase = (int)(World.Time / 24000L % 8L + 8L) % 8;
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

		  //  SkyPlaneEffect.AmbientColor = WorldSkyColor.ToVector3();
		    
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
		    AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
		    ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
		    ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
		     // IndependentBlendEnable = true,
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

			if (World.Dimension == Dimension.Overworld)
		    {
			    var backup = renderArgs.GraphicsDevice.BlendState;
			
			    //renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState;
			    renderArgs.GraphicsDevice.BlendState = CelestialBlendState;
			    
			    DrawSun(renderArgs, renderArgs.Camera.Position);

			    DrawMoon(renderArgs, renderArgs.Camera.Position);
			    
			    renderArgs.GraphicsDevice.BlendState = backup;
		    }

			DrawVoid(renderArgs, renderArgs.Camera.Position);

		    renderArgs.GraphicsDevice.DepthStencilState = depthState;
		    renderArgs.GraphicsDevice.RasterizerState = raster;
		    renderArgs.GraphicsDevice.BlendState = bl;
	    }

	    private void DrawSky(IRenderArgs renderArgs, Vector3 position)
	    {
		    SkyPlaneEffect.FogColor = AtmosphereColor.ToVector3();
		    //SkyPlaneEffect.World = Matrix.CreateRotationX(MathHelper.Pi)
		    //                       * Matrix.CreateTranslation(0, 16, 0)
		    //                       * Matrix.CreateTranslation(position);
		    //   * Matrix.CreateTranslation(position);

		   // SkyPlaneEffect.World = Matrix.CreateRotationX(MathHelper.Pi) * Matrix.CreateTranslation(0, 100, 0)
		   //                                                              * Matrix.CreateRotationX(
			//                                                                 MathHelper.TwoPi * CelestialAngle)  * Matrix.CreateTranslation(position);;

			SkyPlaneEffect.World = Matrix.CreateTranslation(0, 16, 0) * Matrix.CreateTranslation(position);
			
		    SkyPlaneEffect.AmbientColor = WorldSkyColor.ToVector3();

		    // Sky
		    renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);

		    foreach (var pass in SkyPlaneEffect.CurrentTechnique.Passes)
		    {
			    pass.Apply();
			    renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, SkyPlane.VertexCount);
		    }
	    }

	    private void DrawVoid(IRenderArgs renderArgs, Vector3 position)
		{
			// Void
			renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);

			SkyPlaneEffect.World = Matrix.CreateTranslation(0, -16, 0)  * Matrix.CreateTranslation(position);;// * Matrix.CreateTranslation(position);
			//SkyPlaneEffect.DiffuseColor = VoidColor.ToVector3();
			SkyPlaneEffect.AmbientColor = WorldSkyColor.ToVector3()
			                              * new Vector3(0.2f, 0.2f, 0.6f)
			                              + new Vector3(0.04f, 0.04f, 0.1f); // WorldSkyColor.ToVector3();

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
