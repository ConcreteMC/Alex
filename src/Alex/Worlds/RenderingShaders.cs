using Alex.API.Graphics;
using Alex.Entities.Effects;
using Alex.Graphics.Camera;
using Alex.Graphics.Effect;
using Alex.Graphics.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds
{
	public class RenderingShaders
	{
		public BlockEffect AnimatedEffect            { get; }
		public BlockEffect AnimatedTranslucentEffect { get; }
		public BlockEffect TransparentEffect         { get; }
		public BlockEffect TranslucentEffect         { get; }
		public BlockEffect OpaqueEffect              { get; }

		public LightingEffect LightingEffect { get; }
		
		public RenderingShaders(GraphicsDevice device)
		{
			var fogStart = 0f;

			TransparentEffect = new BlockEffect()
			{
				//	Texture = stillAtlas,
				VertexColorEnabled = true,
				World = Matrix.Identity,
				AlphaFunction = CompareFunction.Greater,
				ReferenceAlpha = 32,
				FogStart = fogStart,
				FogEnabled = true,
				
				AmbientLightColor = Color.White,
				AmbientLightDirection = new Vector3(0f, -0.25f, -1f),
				
				// TextureEnabled = true
			};

			TranslucentEffect = new BlockEffect()
			{
				//Texture = stillAtlas,
				VertexColorEnabled = true,
				World = Matrix.Identity,
				AlphaFunction = CompareFunction.Greater,
				ReferenceAlpha = 32,
				FogStart = fogStart,
				FogEnabled = true,
				
				AmbientLightColor = Color.White,
				AmbientLightDirection = new Vector3(0f, -0.25f, -1f),

				//Alpha = 0.5f
			};

			AnimatedEffect = new BlockEffect()
			{
				//	Texture = Resources.Atlas.GetAtlas(0),
				VertexColorEnabled = true,
				World = Matrix.Identity,
				AlphaFunction = CompareFunction.Greater,
				ReferenceAlpha = 32,
				FogStart = fogStart,
				FogEnabled = true,
				
				AmbientLightColor = Color.White,
				AmbientLightDirection = new Vector3(0f, -0.25f, -1f),
				// TextureEnabled = true
			};

			AnimatedTranslucentEffect = new BlockEffect()
			{
				//Texture = Resources.Atlas.GetAtlas(0),
				VertexColorEnabled = true,
				World = Matrix.Identity,
				AlphaFunction = CompareFunction.Greater,
				ReferenceAlpha = 127,
				FogStart = fogStart,
				FogEnabled = true,
				Alpha = 0.5f,
				
				AmbientLightColor = Color.White,
				AmbientLightDirection = new Vector3(0f, -0.25f, -1f),
			};

			OpaqueEffect = new BlockEffect()
			{
				//  TextureEnabled = true,
				//	Texture = stillAtlas,
				FogStart = fogStart,
				VertexColorEnabled = true,
				//  LightingEnabled = true,
				FogEnabled = true,
				ReferenceAlpha = 249,
				AlphaFunction = CompareFunction.Always,
				
				AmbientLightColor = Color.White,
				AmbientLightDirection = new Vector3(0f, -0.25f, -1f),
				//    AlphaFunction = CompareFunction.Greater,
				//    ReferenceAlpha = 127

				//  PreferPerPixelLighting = false
			};
			
			LightingEffect = new LightingEffect(device)
			{
				
			};
		}

		public void SetTextures(Texture2D texture)
		{
			TranslucentEffect.Texture = TransparentEffect.Texture = OpaqueEffect.Texture = texture;
		}

		public void SetAnimatedTextures(Texture2D texture)
		{
			AnimatedTranslucentEffect.Texture = AnimatedEffect.Texture = texture;
		}

		public void Update(SkyBox skyBox, ICamera camera /*Matrix view, Matrix projection*/)
		{
			var view       = camera.ViewMatrix;
			var projection = camera.ProjectionMatrix;
			var cameraPosition = camera.Position;

		/*	var lightPosition = Vector3.Transform(
				cameraPosition,
				Matrix.CreateTranslation(0, 100, 0) * Matrix.CreateRotationX(MathHelper.TwoPi * skyBox.CelestialAngle));

			var lightDirection = lightPosition - cameraPosition;
			
			Matrix lightView = Matrix.CreateLookAt(lightPosition,
				cameraPosition,
				Vector3.Up);

			Matrix  lightProjection = Matrix.CreateOrthographic(100, 100, 0f, camera.FarDistance * 16f);
			*/
			TransparentEffect.View = view;
			TransparentEffect.Projection = projection;
		//	TransparentEffect.LightView = lightView;
		//	TransparentEffect.LightProjection = lightProjection;
			TransparentEffect.CameraPosition = cameraPosition;
			TransparentEffect.CameraFarDistance = camera.FarDistance;
			
			AnimatedEffect.View = view;
			AnimatedEffect.Projection = projection;
		//	AnimatedEffect.LightView = lightView;
		//	AnimatedEffect.LightProjection = lightProjection;
			AnimatedEffect.CameraPosition = cameraPosition;
			AnimatedEffect.CameraFarDistance = camera.FarDistance;

			OpaqueEffect.View = view;
			OpaqueEffect.Projection = projection;
		//	OpaqueEffect.LightView = lightView;
		//	OpaqueEffect.LightProjection = lightProjection;
			OpaqueEffect.CameraPosition = cameraPosition;
			OpaqueEffect.CameraFarDistance = camera.FarDistance;
			
			TranslucentEffect.View = view;
			TranslucentEffect.Projection = projection;
		//	TranslucentEffect.LightView = lightView;
		//	TranslucentEffect.LightProjection = lightProjection;
			TranslucentEffect.CameraPosition = cameraPosition;
			TranslucentEffect.CameraFarDistance = camera.FarDistance;
			
			AnimatedTranslucentEffect.View = view;
			AnimatedTranslucentEffect.Projection = projection;
		//	AnimatedTranslucentEffect.LightView = lightView;
	//		AnimatedTranslucentEffect.LightProjection = lightProjection;
			AnimatedTranslucentEffect.CameraPosition = cameraPosition;
			AnimatedTranslucentEffect.CameraFarDistance = camera.FarDistance;
			
			LightingEffect.View = view;
			LightingEffect.Projection = projection;
			//LightingEffect.CameraPosition = cameraPosition;
		}
		
		public bool FogEnabled
		{
			get { return TransparentEffect.FogEnabled; }
			set
			{
				TransparentEffect.FogEnabled = value;
				TranslucentEffect.FogEnabled = value;
				AnimatedEffect.FogEnabled = value;
				AnimatedTranslucentEffect.FogEnabled = value;
				OpaqueEffect.FogEnabled = value;
			}
		}

		public Vector3 FogColor
		{
			get { return TransparentEffect.FogColor; }
			set
			{
				TransparentEffect.FogColor = value;
				OpaqueEffect.FogColor = value;
				AnimatedEffect.FogColor = value;
				TranslucentEffect.FogColor = value;
				AnimatedTranslucentEffect.FogColor = value;
			}
		}

		public float FogDistance
		{
			get { return TransparentEffect.FogEnd; }
			set
			{
				var fogStart = value - (value / 2);
				TransparentEffect.FogStart = fogStart;
				OpaqueEffect.FogStart = fogStart;
				AnimatedEffect.FogStart = fogStart;
				TranslucentEffect.FogStart = fogStart;
				AnimatedTranslucentEffect.FogStart = fogStart;
				
				TransparentEffect.FogEnd = value;
				OpaqueEffect.FogEnd = value;
				AnimatedEffect.FogEnd = value;
				TranslucentEffect.FogEnd = value;
				AnimatedTranslucentEffect.FogEnd = value;
			}
		}

		public Vector3 AmbientLightColor
		{
			get { return TransparentEffect.DiffuseColor; }
			set
			{
				TransparentEffect.DiffuseColor = value;
				TranslucentEffect.DiffuseColor = value;

				OpaqueEffect.DiffuseColor = value;
				// OpaqueEffect.DiffuseColor = value;
				AnimatedEffect.DiffuseColor = value;
				AnimatedTranslucentEffect.DiffuseColor = value;
			}
		}

		public float BrightnessModifier
		{
			get
			{
				return TransparentEffect.LightOffset;
			}
			set
			{
				TransparentEffect.LightOffset = value;
				TranslucentEffect.LightOffset = value;

				OpaqueEffect.LightOffset = value;
				// OpaqueEffect.DiffuseColor = value;
				AnimatedEffect.LightOffset = value;
				AnimatedTranslucentEffect.LightOffset = value;
				LightingEffect.LightOffset = value;
			}
		}
	}
}