using Alex.Graphics.Effect;
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
				FogEnabled = false,
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
				FogEnabled = false,

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
				FogEnabled = false,
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
				FogEnabled = false,
				Alpha = 0.5f
			};

			OpaqueEffect = new BlockEffect()
			{
				//  TextureEnabled = true,
				//	Texture = stillAtlas,
				FogStart = fogStart,
				VertexColorEnabled = true,
				//  LightingEnabled = true,
				FogEnabled = false,
				ReferenceAlpha = 249
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

		public void UpdateMatrix(Matrix view, Matrix projection)
		{
			TransparentEffect.View = view;
			TransparentEffect.Projection = projection;
		    
			AnimatedEffect.View = view;
			AnimatedEffect.Projection = projection;

			OpaqueEffect.View = view;
			OpaqueEffect.Projection = projection;

			TranslucentEffect.View = view;
			TranslucentEffect.Projection = projection;

			AnimatedTranslucentEffect.View = view;
			AnimatedTranslucentEffect.Projection = projection;

			LightingEffect.View = view;
			LightingEffect.Projection = projection;
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
				var fogStart = value - (value / 4);
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

		public float LightSource1Strength
		{
			get
			{
				return TransparentEffect.LightSource1Strength;
			}
			set
			{
				TransparentEffect.LightSource1Strength = value;
				TranslucentEffect.LightSource1Strength = value;

				OpaqueEffect.LightSource1Strength = value;
				// OpaqueEffect.DiffuseColor = value;
				AnimatedEffect.LightSource1Strength = value;
				AnimatedTranslucentEffect.LightSource1Strength = value;
				//LightingEffect.LightSource1Strength = value;
			}
		}
		
		public Vector3 LightSource1Position
		{
			get
			{
				return TransparentEffect.LightSource1;
			}
			set
			{
				TransparentEffect.LightSource1 = value;
				TranslucentEffect.LightSource1 = value;

				OpaqueEffect.LightSource1 = value;
				// OpaqueEffect.DiffuseColor = value;
				AnimatedEffect.LightSource1 = value;
				AnimatedTranslucentEffect.LightSource1 = value;
				//LightingEffect.LightSource1Strength = value;
			}
		}
	}
}