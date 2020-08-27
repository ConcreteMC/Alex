using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Effect
{
	public class SkyEffect : Microsoft.Xna.Framework.Graphics.Effect, IEffectMatrices
    {
        #region Effect Parameters

        EffectParameter _textureParam;
       // EffectParameter diffuseColorParam;
        EffectParameter         _alphaTestParam;
        EffectParameter         _fogColorParam;
       // EffectParameter         fogVectorParam;
       private  EffectParameter _fogStartParam;
       private  EffectParameter _fogEndParam;
        private EffectParameter _fogEnabledParam;

        private EffectParameter _worldParam;

        private EffectParameter _projParam;

        private EffectParameter _viewParam;

        private EffectParameter _lightOffsetParam;
         //EffectParameter worldViewProjParam;
         private EffectParameter _diffuseColorParam;
         private EffectParameter _ambientColorParam;
         
         #endregion

        #region Fields

        private Vector3 _ambientColor = Vector3.Zero;
        private Vector3 _diffuseColor = Vector3.Zero;

        private bool    _fogEnabled = false;
        private Vector3 _fogColor = Vector3.Zero;

        Matrix _world      = Matrix.Identity;
        Matrix _view       = Matrix.Identity;
        Matrix _projection = Matrix.Identity;

        Matrix _worldView;

        float _alpha = 1;

        float _fogStart = 0;
        float _fogEnd   = 1;

        CompareFunction _alphaFunction = CompareFunction.Greater;
        int             _referenceAlpha;
        bool            _isEqNe;

        EffectDirtyFlags _dirtyFlags = EffectDirtyFlags.All;

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

        public bool FogEnabled
        {
            get => _fogEnabled;
            set
            {
                _fogEnabled = value;
                _dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }
        
        public float FogStart
        {
            get => _fogStart;
            set
            {
                _fogStart = value;
                _dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }
        
        public float FogEnd
        {
            get => _fogEnd;
            set
            {
                _fogEnd = value;
                _dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }

        public Vector3 FogColor
        {
            get => _fogColor;
            set
            {
                _fogColor = value;
                _dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }

        public Vector3 DiffuseColor
        {
            get => _diffuseColor;
            set
            {
                _diffuseColor = value;
                _dirtyFlags |= EffectDirtyFlags.DiffuseColor;
            }
        }

        public Vector3 AmbientColor
        {
            get => _ambientColor;
            set
            {
                _ambientColor = value;
                _dirtyFlags |= EffectDirtyFlags.AmbientColor;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new AlphaTestEffect with default parameter settings.
        /// </summary>
        public SkyEffect(GraphicsDevice device, byte[] byteCode) : base(device, byteCode)
        {
            CacheEffectParameters();
        }
        
        public SkyEffect(GraphicsDevice device) : base(ResourceManager.SkyEffect)
        {
            CacheEffectParameters();
        }
        
        /// <summary>
        /// Creates a new AlphaTestEffect by cloning parameter settings from an existing instance.
        /// </summary>
        public SkyEffect(SkyEffect cloneSource) : base(cloneSource)
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
            return new SkyEffect(this);
        }

        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            _alphaTestParam = Parameters["AlphaTest"];

            _worldParam = Parameters["World"];

            _projParam = Parameters["Projection"];

            _viewParam = Parameters["View"];

           // lightOffsetParam = Parameters["LightOffset"];

            _fogEnabledParam = Parameters["FogEnabled"];

            _fogEndParam = Parameters["FogEnd"];
            _fogStartParam = Parameters["FogStart"];
            _fogColorParam = Parameters["FogColor"];

            _diffuseColorParam = Parameters["DiffuseColor"];
            _ambientColorParam = Parameters["AmbientColor"];
        }

        internal static EffectDirtyFlags SetWorldViewProjAndFog(
            EffectDirtyFlags dirtyFlags,
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
                _dirtyFlags, ref _world, ref _view, ref _projection, ref _worldView,
                _worldParam, _viewParam, _projParam);

            if ((_dirtyFlags & EffectDirtyFlags.LightOffset) != 0)
            {
           //     lightOffsetParam.SetValue((float)_lightOffset);
                _dirtyFlags &= ~EffectDirtyFlags.LightOffset;
            }

            // Recompute the shader index?
            if ((_dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                _dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                CurrentTechnique = Techniques[0];
            }

            if ((_dirtyFlags & EffectDirtyFlags.Fog) != 0)
            {
                _dirtyFlags &= ~EffectDirtyFlags.Fog;
                _fogEnabledParam.SetValue(FogEnabled ? 1f : 0f);
                _fogStartParam.SetValue(FogStart);
                _fogEndParam.SetValue(FogEnd);
                _fogColorParam.SetValue(_fogColor);
            }

            if ((_dirtyFlags & EffectDirtyFlags.DiffuseColor) != 0)
            {
                _dirtyFlags &= ~EffectDirtyFlags.DiffuseColor;
               // _diffuseColorParam.SetValue(_diffuseColor);
            }

            if ((_dirtyFlags & EffectDirtyFlags.AmbientColor) != 0)
            {
                _dirtyFlags &= ~EffectDirtyFlags.AmbientColor;
                _ambientColorParam.SetValue(_ambientColor);
            }
        }


        #endregion
    }
}