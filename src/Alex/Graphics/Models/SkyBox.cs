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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using MathF = System.MathF;

namespace Alex.Graphics.Models
{
	//Thanks https://github.com/SirCmpwn/TrueCraft
	public class SkyBox
	{
		private float MoonX = 1f/4f;
		private float MoonY = 1f/2f;

		private BasicEffect SkyPlaneEffect { get; set; }
	    private BasicEffect CelestialPlaneEffect { get; set; }
		private BasicEffect CloudsPlaneEffect { get; set; }

	    private VertexBuffer CloudsPlane { get; set; }
        private VertexBuffer SkyPlane { get; set; }
	    private VertexBuffer CelestialPlane { get; set; }
		private VertexBuffer MoonPlane { get; }

		private Texture2D SunTexture { get; }
		private Texture2D MoonTexture { get; }
		private Texture2D CloudTexture { get; }

		private bool CanRender { get; set; } = true;
		public bool EnableClouds { get; set; } = false;

		private World World { get; }

	    private readonly VertexPositionTexture[] _moonPlaneVertices;
		private Alex Game { get; }
		private IOptionsProvider OptionsProvider { get; }
		private AlexOptions Options => OptionsProvider.AlexOptions;
		public SkyBox(Alex alex, GraphicsDevice device, World world)
		{
			World = world;
			Game = alex;
			OptionsProvider = alex.Services.GetService<IOptionsProvider>();

		    if (alex.Resources.ResourcePack.TryGetTexture("environment/sun", out Texture2D sun))
		    {
			    SunTexture = sun;
		    }
		    else
		    {
			    CanRender = false;
			    return;
		    }

		    if (alex.Resources.ResourcePack.TryGetTexture("environment/moon_phases", out Texture2D moonPhases))
		    {
			    MoonTexture = moonPhases;
		    }
		    else
		    {
			    CanRender = false;
			    return;
		    }

		    if (alex.Resources.ResourcePack.TryGetTexture("environment/clouds", out Texture2D cloudTexture))
		    {
			    CloudTexture = cloudTexture;
			    EnableClouds = false;
		    }
		    else
		    {
			    EnableClouds = false;
		    }

            var d = Options.VideoOptions.RenderDistance ^ 2;

			CelestialPlaneEffect = new BasicEffect(device);
			CelestialPlaneEffect.TextureEnabled = true;

			SkyPlaneEffect = new BasicEffect(device);
			SkyPlaneEffect.VertexColorEnabled = false;
			SkyPlaneEffect.FogEnabled = true;
			SkyPlaneEffect.FogStart = 0;
			SkyPlaneEffect.FogEnd = d * 0.8f;
			SkyPlaneEffect.LightingEnabled = true;

			var planeDistance = d * 3;
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
		}

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

	    private Color WorldSkyColor
	    {
		    get
		    {
			    var position = World.Camera.Position;

			    float f1 = MathF.Cos(CelestialAngle * ((float)Math.PI * 2F)) * 2.0F + 0.5F;
			    f1 = MathHelper.Clamp(f1, 0.0F, 1.0F);

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
			    r = r * f1;
			    g = g * f1;
			    b = b * f1;

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

	    public Color AtmosphereColor
		{
			get
			{
				float blendFactor = ((Options.VideoOptions.RenderDistance ^2) / 100f) * 0.45f;

				float Blend(float source, float destination) => destination + (source - destination) * blendFactor;

				var fog = WorldFogColor.ToVector3();
				var sky = WorldSkyColor.ToVector3();
				var color = new Vector3(Blend(sky.X, fog.X), Blend(sky.Y, fog.Y), Blend(sky.Z, fog.Z));
				// TODO: more stuff
				return new Color(color);
			}
		}

	    private int CurrentMoonPhase = 0;
	    public void Update(IUpdateArgs args)
	    {
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

			    var modified = _moonPlaneVertices.Select(x => x.TextureCoordinate).ToArray();
			    MoonPlane.SetData(12, modified, 0, modified.Length, MoonPlane.VertexDeclaration.VertexStride);
		    }
	    }

	    public void Draw(IRenderArgs renderArgs)
	    {
		    if (!CanRender) return;
		    var camera = renderArgs.Camera;
			
			renderArgs.GraphicsDevice.Clear(AtmosphereColor);

			SkyPlaneEffect.View = camera.ViewMatrix;
			SkyPlaneEffect.Projection = camera.ProjectionMatrix;

		    CelestialPlaneEffect.View = camera.ViewMatrix;
		    CelestialPlaneEffect.Projection = camera.ProjectionMatrix;

		    if (EnableClouds)
		    {
			    CloudsPlaneEffect.View = camera.ViewMatrix;
			    CloudsPlaneEffect.Projection = camera.ProjectionMatrix;
		    }


		    var depthState = renderArgs.GraphicsDevice.DepthStencilState;
		    var raster = renderArgs.GraphicsDevice.RasterizerState;
		    var bl = renderArgs.GraphicsDevice.BlendState;

			renderArgs.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = false };
			renderArgs.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
			renderArgs.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			
			DrawSky(renderArgs, camera.Position);

			var backup = renderArgs.GraphicsDevice.BlendState;
		    renderArgs.GraphicsDevice.BlendState = BlendState.Additive;
		    renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

		    DrawSun(renderArgs, camera.Position);

			DrawMoon(renderArgs, camera.Position);

			renderArgs.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            if (EnableClouds)
				DrawClouds(renderArgs, camera.Position);

			renderArgs.GraphicsDevice.BlendState = backup;

			DrawVoid(renderArgs, camera.Position);

		    renderArgs.GraphicsDevice.DepthStencilState = depthState;
		    renderArgs.GraphicsDevice.RasterizerState = raster;
		    renderArgs.GraphicsDevice.BlendState = bl;
	    }

		private void DrawSky(IRenderArgs renderArgs, Vector3 position)
		{
			// Sky
			SkyPlaneEffect.FogColor = AtmosphereColor.ToVector3();
			SkyPlaneEffect.World = Matrix.CreateRotationX(MathHelper.Pi)
			                       * Matrix.CreateTranslation(0, 100, 0)
			                       * Matrix.CreateRotationX(MathHelper.TwoPi * CelestialAngle) * Matrix.CreateTranslation(position);
			SkyPlaneEffect.AmbientLightColor = WorldSkyColor.ToVector3();

			renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);
			foreach (var pass in SkyPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
			}
			renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
		}

		private void DrawClouds(IRenderArgs renderArgs, Vector3 position)
		{
			// Clouds
			CloudsPlaneEffect.Texture = CloudTexture;
			CloudsPlaneEffect.World = Matrix.CreateTranslation(position.X, 127, position.Z);
            //CloudsPlaneEffect.AmbientLightColor = WorldSkyColor.ToVector3();

            renderArgs.GraphicsDevice.SetVertexBuffer(CloudsPlane);
			foreach (var pass in CloudsPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
			}
			renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
		}

        private void DrawSun(IRenderArgs renderArgs, Vector3 position)
		{
			// Sun
			CelestialPlaneEffect.Texture = SunTexture;
			CelestialPlaneEffect.World = Matrix.CreateRotationX(MathHelper.Pi)
			                             * Matrix.CreateTranslation(0, 100, 0)
			                             * Matrix.CreateRotationX(MathHelper.TwoPi * CelestialAngle) * Matrix.CreateTranslation(position);

			renderArgs.GraphicsDevice.SetVertexBuffer(CelestialPlane);
			foreach (var pass in CelestialPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
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
			SkyPlaneEffect.World = Matrix.CreateTranslation(0, -4, 0) * Matrix.CreateTranslation(position);
			SkyPlaneEffect.AmbientLightColor = WorldSkyColor.ToVector3()
			                                   * new Vector3(0.2f, 0.2f, 0.6f)
			                                   + new Vector3(0.04f, 0.04f, 0.1f);
			foreach (var pass in SkyPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
			}
			renderArgs.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
		}
	}
}
