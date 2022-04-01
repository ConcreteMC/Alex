using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Effect
{
	public class LightingEffect : Microsoft.Xna.Framework.Graphics.Effect, IEffectMatrices
	{
		#region Effect Parameters

		private EffectParameter _alphaTestParam;

		private EffectParameter _worldParam;

		private EffectParameter _projParam;

		private EffectParameter _viewParam;

		private EffectParameter _lightOffsetParam;

		#endregion

		#region Fields

		private Matrix _world = Matrix.Identity;
		private Matrix _view = Matrix.Identity;
		private Matrix _projection = Matrix.Identity;

		private Matrix _worldView;

		private float _alpha = 1;

		private float _fogStart = 0;
		private float _fogEnd = 1;

		private CompareFunction _alphaFunction = CompareFunction.Greater;
		private int _referenceAlpha;
		private bool _isEqNe;

		private EffectDirtyFlags _dirtyFlags = EffectDirtyFlags.All;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the world matrix.
		/// </summary>
		public Matrix World
		{
			get { return _world; }

			set
			{
				_world = value;
				_dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
			}
		}


		/// <summary>
		/// Gets or sets the view matrix.
		/// </summary>
		public Matrix View
		{
			get { return _view; }

			set
			{
				_view = value;
				_dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
			}
		}


		/// <summary>
		/// Gets or sets the projection matrix.
		/// </summary>
		public Matrix Projection
		{
			get { return _projection; }

			set
			{
				_projection = value;
				_dirtyFlags |= EffectDirtyFlags.WorldViewProj;
			}
		}


		/// <summary>
		/// Gets or sets the material alpha.
		/// </summary>
		public float Alpha
		{
			get { return _alpha; }

			set
			{
				_alpha = value;
				_dirtyFlags |= EffectDirtyFlags.MaterialColor;
			}
		}

		private float _lightOffset = 0;

		public float LightOffset
		{
			get
			{
				return _lightOffset;
			}
			set
			{
				_lightOffset = value;
				_dirtyFlags |= EffectDirtyFlags.LightOffset;
			}
		}

		/// <summary>
		/// Gets or sets the alpha compare function (default Greater).
		/// </summary>
		public CompareFunction AlphaFunction
		{
			get { return _alphaFunction; }

			set
			{
				_alphaFunction = value;
				_dirtyFlags |= EffectDirtyFlags.AlphaTest;
			}
		}


		/// <summary>
		/// Gets or sets the reference alpha value (default 0).
		/// </summary>
		public int ReferenceAlpha
		{
			get { return _referenceAlpha; }

			set
			{
				_referenceAlpha = value;
				_dirtyFlags |= EffectDirtyFlags.AlphaTest;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates a new AlphaTestEffect with default parameter settings.
		/// </summary>
		public LightingEffect(GraphicsDevice device, byte[] byteCode) : base(device, byteCode)
		{
			CacheEffectParameters();
		}

		public LightingEffect(GraphicsDevice device) : base(ResourceManager.LightingEffect)
		{
			CacheEffectParameters();
		}

		/// <summary>
		/// Creates a new AlphaTestEffect by cloning parameter settings from an existing instance.
		/// </summary>
		public LightingEffect(LightingEffect cloneSource) : base(cloneSource)
		{
			CacheEffectParameters();

			_world = cloneSource._world;
			_view = cloneSource._view;
			_projection = cloneSource._projection;

			_alpha = cloneSource._alpha;

			_fogStart = cloneSource._fogStart;
			_fogEnd = cloneSource._fogEnd;

			_alphaFunction = cloneSource._alphaFunction;
			_referenceAlpha = cloneSource._referenceAlpha;
		}

		/// <summary>
		/// Creates a clone of the current AlphaTestEffect instance.
		/// </summary>
		public override Microsoft.Xna.Framework.Graphics.Effect Clone()
		{
			return new LightingEffect(this);
		}

		/// <summary>
		/// Looks up shortcut references to our effect parameters.
		/// </summary>
		private void CacheEffectParameters()
		{
			_alphaTestParam = Parameters["AlphaTest"];

			_worldParam = Parameters["World"];

			_projParam = Parameters["Projection"];

			_viewParam = Parameters["View"];

			_lightOffsetParam = Parameters["LightOffset"];
		}

		internal static EffectDirtyFlags SetWorldViewProjAndFog(EffectDirtyFlags dirtyFlags,
			ref Matrix world,
			ref Matrix view,
			ref Matrix projection,
			ref Matrix worldView,
			EffectParameter worldParam,
			EffectParameter viewParam,
			EffectParameter projectionParam)
		{
			if ((dirtyFlags & EffectDirtyFlags.WorldViewProj) != ~EffectDirtyFlags.All)
			{
				worldParam.SetValue(world);
				viewParam.SetValue(view);
				projectionParam.SetValue(projection);
				dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
			}

			return dirtyFlags;
		}

		/// <summary>
		/// Lazily computes derived parameter values immediately before applying the effect.
		/// </summary>
		protected override void OnApply()
		{
			// Recompute the world+view+projection matrix or fog vector?
			_dirtyFlags = SetWorldViewProjAndFog(
				_dirtyFlags, ref _world, ref _view, ref _projection, ref _worldView, _worldParam, _viewParam, _projParam);

			if ((_dirtyFlags & EffectDirtyFlags.LightOffset) != 0)
			{
				_lightOffsetParam.SetValue((float)_lightOffset);
				_dirtyFlags &= ~EffectDirtyFlags.LightOffset;
			}

			// Recompute the alpha test settings?
			if ((_dirtyFlags & EffectDirtyFlags.AlphaTest) != 0)
			{
				Vector4 alphaTest = new Vector4();
				bool eqNe = false;

				// Convert reference alpha from 8 bit integer to 0-1 float format.
				float reference = (float)_referenceAlpha / 255f;

				// Comparison tolerance of half the 8 bit integer precision.
				const float threshold = 0.5f / 255f;

				switch (_alphaFunction)
				{
					case CompareFunction.Less:
						// Shader will evaluate: clip((a < x) ? z : w)
						alphaTest.X = reference - threshold;
						alphaTest.Z = 1;
						alphaTest.W = -1;

						break;

					case CompareFunction.LessEqual:
						// Shader will evaluate: clip((a < x) ? z : w)
						alphaTest.X = reference + threshold;
						alphaTest.Z = 1;
						alphaTest.W = -1;

						break;

					case CompareFunction.GreaterEqual:
						// Shader will evaluate: clip((a < x) ? z : w)
						alphaTest.X = reference - threshold;
						alphaTest.Z = -1;
						alphaTest.W = 1;

						break;

					case CompareFunction.Greater:
						// Shader will evaluate: clip((a < x) ? z : w)
						alphaTest.X = reference + threshold;
						alphaTest.Z = -1;
						alphaTest.W = 1;

						break;

					case CompareFunction.Equal:
						// Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
						alphaTest.X = reference;
						alphaTest.Y = threshold;
						alphaTest.Z = 1;
						alphaTest.W = -1;
						eqNe = true;

						break;

					case CompareFunction.NotEqual:
						// Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
						alphaTest.X = reference;
						alphaTest.Y = threshold;
						alphaTest.Z = -1;
						alphaTest.W = 1;
						eqNe = true;

						break;

					case CompareFunction.Never:
						// Shader will evaluate: clip((a < x) ? z : w)
						alphaTest.Z = -1;
						alphaTest.W = -1;

						break;

					case CompareFunction.Always:
					default:
						// Shader will evaluate: clip((a < x) ? z : w)
						alphaTest.Z = 1;
						alphaTest.W = 1;

						break;
				}

				_alphaTestParam.SetValue(alphaTest);

				_dirtyFlags &= ~EffectDirtyFlags.AlphaTest;

				// If we changed between less/greater vs. equal/notequal
				// compare modes, we must also update the shader index.
				if (_isEqNe != eqNe)
				{
					_isEqNe = eqNe;
					_dirtyFlags |= EffectDirtyFlags.ShaderIndex;
				}
			}

			// Recompute the shader index?
			if ((_dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
			{
				_dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

				CurrentTechnique = Techniques[0];
			}
		}

		#endregion
	}
}