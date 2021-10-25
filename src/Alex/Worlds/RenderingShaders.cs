using Alex.Common.Graphics;
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

		public BlockEffect TransparentEffect         { get; }
		public BlockEffect OpaqueEffect              { get; }
		
		
		public RenderingShaders()
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
				ApplyAnimations = true
				// TextureEnabled = true
			};

			OpaqueEffect = new BlockEffect()
			{
				FogStart = fogStart,
				VertexColorEnabled = true,
				//  LightingEnabled = true,
				FogEnabled = true,
				ReferenceAlpha = 249,
				AlphaFunction = CompareFunction.Always,
				
				AmbientLightColor = Color.White,
				AmbientLightDirection = new Vector3(0f, -0.25f, -1f),
			};
		}

		public void SetTextures(Texture2D texture)
		{
			AnimatedEffect.Texture =
				TransparentEffect.Texture = OpaqueEffect.Texture = texture;
		}


		private float _timer = 0f;

		public void Update(float dt, ICamera camera /*Matrix view, Matrix projection*/)
		{
			_timer += dt;

			if (_timer >= (1.0f / 12))
			{
				_timer -= 1.0f / 12;
				AnimatedEffect.Frame++;
			}

			var view = camera.ViewMatrix;
			var projection = camera.ProjectionMatrix;
			var cameraPosition = camera.Position;

			OpaqueEffect.View = AnimatedEffect.View = TransparentEffect.View = view;

			OpaqueEffect.Projection =
				AnimatedEffect.Projection = TransparentEffect.Projection = projection;

			OpaqueEffect.CameraPosition =
				AnimatedEffect.CameraPosition = TransparentEffect.CameraPosition = cameraPosition;

			OpaqueEffect.CameraFarDistance = AnimatedEffect.CameraFarDistance =
				TransparentEffect.CameraFarDistance = camera.FarDistance;
		}

		public bool FogEnabled
		{
			get { return TransparentEffect.FogEnabled; }
			set
			{
				TransparentEffect.FogEnabled = value;
				AnimatedEffect.FogEnabled = value;
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
				
				TransparentEffect.FogEnd = value;
				OpaqueEffect.FogEnd = value;
				AnimatedEffect.FogEnd = value;
			}
		}

		public Vector3 AmbientLightColor
		{
			get { return TransparentEffect.DiffuseColor; }
			set
			{
				TransparentEffect.DiffuseColor = value;
				OpaqueEffect.DiffuseColor = value;
				AnimatedEffect.DiffuseColor = value;
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
				OpaqueEffect.LightOffset = value;
				AnimatedEffect.LightOffset = value;
			}
		}
	}
}