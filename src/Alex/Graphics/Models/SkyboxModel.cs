using System;
using System.Collections.Generic;
using System.Drawing;
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
		public SkyboxModel(Alex alex, GraphicsDevice device, World world)
		{
			World = world;

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

			CelestialPlaneEffect = new BasicEffect(device);
			CelestialPlaneEffect.TextureEnabled = true;

			SkyPlaneEffect = new BasicEffect(device);
			SkyPlaneEffect.VertexColorEnabled = false;
			SkyPlaneEffect.FogEnabled = true;
			SkyPlaneEffect.FogStart = 0;
			SkyPlaneEffect.FogEnd = 64 * 0.8f;
			SkyPlaneEffect.LightingEnabled = true;
			var plane = new[]
			{
				new VertexPositionColor(new Vector3(-64, 0, -64), Color.White),
				new VertexPositionColor(new Vector3(64, 0, -64), Color.White),
				new VertexPositionColor(new Vector3(-64, 0, 64), Color.White),

				new VertexPositionColor(new Vector3(64, 0, -64), Color.White),
				new VertexPositionColor(new Vector3(64, 0, 64), Color.White),
				new VertexPositionColor(new Vector3(-64, 0, 64), Color.White)
			};
			SkyPlane = new VertexBuffer(device, VertexPositionColor.VertexDeclaration,
				plane.Length, BufferUsage.WriteOnly);
			SkyPlane.SetData<VertexPositionColor>(plane);

			var celestialPlane = new[]
			{
				new VertexPositionTexture(new Vector3(-60, 0, -60), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(60, 0, -60), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(-60, 0, 60), new Vector2(0, 1)),

				new VertexPositionTexture(new Vector3(60, 0, -60), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(60, 0, 60), new Vector2(1, 1)),
				new VertexPositionTexture(new Vector3(-60, 0, 60), new Vector2(0, 1))
			};
			CelestialPlane = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration,
				celestialPlane.Length, BufferUsage.WriteOnly);
			CelestialPlane.SetData<VertexPositionTexture>(celestialPlane);

			MoonPlaneVertices = new[]
			{
				new VertexPositionTexture(new Vector3(-60, 0, -60), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(60, 0, -60), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(-60, 0, 60), new Vector2(0, 1)),
				new VertexPositionTexture(new Vector3(60, 0, -60), new Vector2(1, 0)),
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

	    public Color WorldSkyColor => new Color(World.GetSkyColor(CelestialAngle));

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
				const float blendFactor = 0.29f; // TODO: Compute based on view distance

				float Blend(float source, float destination) => destination + (source - destination) * blendFactor;

				var fog = WorldFogColor.ToVector3();
				var sky = WorldSkyColor.ToVector3();
				var color = new Vector3(Blend(sky.X, fog.X), Blend(sky.Y, fog.Y), Blend(sky.Z, fog.Z));
				// TODO: more stuff
				return new Color(color);
			}
		}

	    public void Update(GameTime gameTime)
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

		    MoonPlane.SetData<VertexPositionTexture>(MoonPlaneVertices);
		}

	    public void Draw(IRenderArgs renderArgs, Camera camera)
	    {
		    if (!CanRender) return;

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
		    renderArgs.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			// Void
		    renderArgs.GraphicsDevice.SetVertexBuffer(SkyPlane);
			SkyPlaneEffect.World = Matrix.CreateTranslation(camera.Position.X, -16, camera.Position.Z);
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
