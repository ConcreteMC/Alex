using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Alex.API.Graphics;
using Alex.Gamestates;
using Alex.Rendering.Camera;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Graphics.Models
{
	//Thanks https://github.com/SirCmpwn/TrueCraft
	public class SkyboxModel
    {
	    private BasicEffect SkyPlaneEffect { get; set; }
	    private BasicEffect CelestialPlaneEffect { get; set; }
	    private VertexBuffer SkyPlane { get; set; }
	    private VertexBuffer CelestialPlane { get; set; }
		private VertexBuffer MoonPlane { get; }

		private Texture2D SunTexture { get; }
		private Texture2D MoonTexture { get; }

	    private bool CanRender { get; set; } = true;
	    public float BiomeTemperature { get; set; } = 0.8f;

		private World World { get; }

	    private VertexPositionTexture[] MoonPlaneVertices;
		private Alex Game { get; }
		public SkyboxModel(Alex alex, GraphicsDevice device, World world)
		{
			World = world;
			Game = alex;

		    if (alex.Resources.BedrockResourcePack.TryGetTexture("textures/environment/sun", out Bitmap sun))
		    {
			    SunTexture = TextureUtils.BitmapToTexture2D(device, sun);
			}
		    else
		    {
			    CanRender = false;
			    return;
		    }

		    if (alex.Resources.BedrockResourcePack.TryGetTexture("textures/environment/moon_phases", out Bitmap moonPhases))
		    {
			    MoonTexture = TextureUtils.BitmapToTexture2D(device, moonPhases);
		    }
		    else
		    {
			    CanRender = false;
			    return;
		    }

			var d = alex.GameSettings.RenderDistance ^ 2;

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
			SkyPlane = new VertexBuffer(device, VertexPositionColor.VertexDeclaration,
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
			CelestialPlane = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration,
				celestialPlane.Length, BufferUsage.WriteOnly);
			CelestialPlane.SetData<VertexPositionTexture>(celestialPlane);

			MoonPlaneVertices = new[]
			{
				new VertexPositionTexture(new Vector3(-planeDistance, 0, -planeDistance), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, 1)),

				new VertexPositionTexture(new Vector3(planeDistance, 0, -planeDistance), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(planeDistance, 0, planeDistance), new Vector2(1, 1)),
				new VertexPositionTexture(new Vector3(-planeDistance, 0, planeDistance), new Vector2(0, 1)),
			};
			MoonPlane = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration,
				MoonPlaneVertices.Length, BufferUsage.WriteOnly);
			MoonPlane.SetData<VertexPositionTexture>(MoonPlaneVertices);
		}

		private float CelestialAngle
		{
			get
			{
				float x = (World.WorldTime % 24000f) / 24000f - 0.25f;
				return (x + (MathF.Cos(x * 3.14159265358979f) * -0.5f + 0.5f - x) / 3.0f) * 6.28318530717959f;
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
			    /*float f6 = 0;//RainStrength

			    if (f6 > 0.0F)
			    {
				    float f7 = (r * 0.3F + g * 0.59F + b * 0.11F) * 0.6F;
				    float f8 = 1.0F - f6 * 0.75F;
				    r = r * f8 + f7 * (1.0F - f8);
				    g = g * f8 + f7 * (1.0F - f8);
				    b = b * f8 + f7 * (1.0F - f8);
			    }

			    float f10 = 0f; //Thunder

			    if (f10 > 0.0F)
			    {
				    float f11 = (r * 0.3F + g * 0.59F + b * 0.11F) * 0.2F;
				    float f9 = 1.0F - f10 * 0.75F;
				    r = r * f9 + f11 * (1.0F - f9);
				    g = g * f9 + f11 * (1.0F - f9);
				    b = b * f9 + f11 * (1.0F - f9);
			    }

			    if (LastLightningBolt > 0)
			    {
				    float f12 = (float)this.LastLightningBolt - Tick;

				    if (f12 > 1.0F)
				    {
					    f12 = 1.0F;
				    }

				    f12 = f12 * 0.45F;
				    r = r * (1.0F - f12) + 0.8F * f12;
				    g = g * (1.0F - f12) + 0.8F * f12;
				    b = b * (1.0F - f12) + 1.0F * f12;
			    }*/

				return new Color(r,g,b);
			//    return new Vector3(r, g, b);
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
				float blendFactor = ((Game.GameSettings.RenderDistance ^2) / 100f) * 0.45f;//  0.29f; // TODO: Compute based on view distance

				float Blend(float source, float destination) => destination + (source - destination) * blendFactor;

				var fog = WorldFogColor.ToVector3();
				var sky = WorldSkyColor.ToVector3();
				var color = new Vector3(Blend(sky.X, fog.X), Blend(sky.Y, fog.Y), Blend(sky.Z, fog.Z));
				// TODO: more stuff
				return new Color(color);
			}
		}

	    public void Update(IUpdateArgs args)
	    {
		    var moonPhase = (int)(World.WorldTime / 24000L % 8L + 8L) % 8;
		    int i2 = moonPhase % 4;
		    int k2 = moonPhase / 4 % 2;
		    float f22 = (i2 + 0) / 4.0F;
		    float f23 = (k2 + 0) / 2.0F;
		    float f24 = (i2 + 1) / 4.0F;
		    float f14 = (k2 + 1) / 2.0F;

		    MoonPlaneVertices[0] = new VertexPositionTexture(new Vector3(-20, 0, 20), new Vector2(f24, f14));
		    MoonPlaneVertices[1] = new VertexPositionTexture(new Vector3(20, 0, 20), new Vector2(f22, f14));
		    MoonPlaneVertices[2] = new VertexPositionTexture(new Vector3(20, 0, -20), new Vector2(f22, f23));
		    MoonPlaneVertices[3] = new VertexPositionTexture(new Vector3(-20, 0, -20), new Vector2(f24, f23));
			MoonPlaneVertices[4] = new VertexPositionTexture(new Vector3(20, 0, 20), new Vector2(f24, f23));
		    MoonPlaneVertices[5] = new VertexPositionTexture(new Vector3(-20, 0, 20), new Vector2(f24, f23));

			MoonPlane.SetData<VertexPositionTexture>(MoonPlaneVertices);
		}

	    public void Draw(IRenderArgs renderArgs)
	    {
		    if (!CanRender) return;
		    var camera = renderArgs.Camera;

			renderArgs.GraphicsDevice.Clear(AtmosphereColor);
		    renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);

		    SkyPlaneEffect.View = camera.ViewMatrix;
			SkyPlaneEffect.Projection = camera.ProjectionMatrix;

		    CelestialPlaneEffect.View = camera.ViewMatrix;
		    CelestialPlaneEffect.Projection = camera.ProjectionMatrix;

		    var depthState = renderArgs.GraphicsDevice.DepthStencilState;
		    var raster = renderArgs.GraphicsDevice.RasterizerState;
		    var bl = renderArgs.GraphicsDevice.BlendState;

			renderArgs.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = false };
			renderArgs.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
			renderArgs.GraphicsDevice.BlendState = BlendState.AlphaBlend;

			// Sky
			
			SkyPlaneEffect.FogColor = AtmosphereColor.ToVector3();
			SkyPlaneEffect.World =  Matrix.CreateRotationX(MathHelper.Pi)
				* Matrix.CreateTranslation(0, 100, 0)
				* Matrix.CreateRotationX(MathHelper.TwoPi * CelestialAngle) * Matrix.CreateTranslation(camera.Position);
			SkyPlaneEffect.AmbientLightColor = WorldSkyColor.ToVector3();
		    foreach (var pass in SkyPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
			}
		    SkyPlaneEffect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

			// Sun
			renderArgs.GraphicsDevice.SetVertexBuffer(CelestialPlane);

			var backup = renderArgs.GraphicsDevice.BlendState;
		    renderArgs.GraphicsDevice.BlendState = BlendState.Additive;
		    renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

			CelestialPlaneEffect.Texture = SunTexture;
			CelestialPlaneEffect.World = Matrix.CreateRotationX(MathHelper.Pi)
				* Matrix.CreateTranslation(0, 100, 0)
				* Matrix.CreateRotationX(MathHelper.TwoPi * CelestialAngle) * Matrix.CreateTranslation(camera.Position) ;

			foreach (var pass in CelestialPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
			}
		    CelestialPlaneEffect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

			renderArgs.GraphicsDevice.SetVertexBuffer(MoonPlane);

			// Moon
			CelestialPlaneEffect.Texture = MoonTexture;
			CelestialPlaneEffect.World =  Matrix.CreateTranslation(0, -100, 0)
				* Matrix.CreateRotationX(MathHelper.TwoPi * CelestialAngle) * Matrix.CreateTranslation(camera.Position) ;
			foreach (var pass in CelestialPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
			}
		    CelestialPlaneEffect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

			renderArgs.GraphicsDevice.BlendState = backup;
		//    renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			// Void
		    renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);
		    SkyPlaneEffect.World = Matrix.CreateTranslation(0, -4, 0) * Matrix.CreateTranslation(camera.Position);
			SkyPlaneEffect.AmbientLightColor = WorldSkyColor.ToVector3()
				* new Vector3(0.2f, 0.2f, 0.6f)
				+ new Vector3(0.04f, 0.04f, 0.1f);
			foreach (var pass in SkyPlaneEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
			}
		    SkyPlaneEffect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

			renderArgs.GraphicsDevice.DepthStencilState = depthState;
		    renderArgs.GraphicsDevice.RasterizerState = raster;
		    renderArgs.GraphicsDevice.BlendState = bl;
	    }
    }
}
