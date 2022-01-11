using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Effect
{
    [Flags]
    internal enum EffectDirtyFlags
    {
        WorldViewProj = 1,
        World         = 2,
        EyePosition   = 4,
        MaterialColor = 8,
        Fog           = 16, // 0x00000010
        FogEnable     = 32, // 0x00000020
        AlphaTest     = 64, // 0x00000040
        ShaderIndex   = 128, // 0x00000080
        LightOffset   = 256,
        AmbientColor  = 512,
        LightDirection = 1024,
        CameraPosition = 2048,
        CameraFarDistance = 4096,
        LightViewProjection = 8192,
        Frame = 16384,
       // LightSource1  = 512,
        All           = -1, // 0xFFFFFFFF
    }

    public class BlockEffect : Microsoft.Xna.Framework.Graphics.Effect, IEffectMatrices, IEffectFog
    {
        #region Effect Parameters

        private EffectParameter _textureParam;

        private EffectParameter _applyAnimationsParameter;
        private EffectParameter _textureScaleParam;
       // EffectParameter diffuseColorParam;
        private EffectParameter _alphaTestParam;
        private EffectParameter _fogColorParam;
        private EffectParameter _fogStartParam;
        private EffectParameter _fogEndParam;
        private EffectParameter _fogEnabledParam;
        
        private EffectParameter _fogVectorParam;

        private EffectParameter _worldParam;

        private EffectParameter _projParam;

        private EffectParameter _viewParam;

        private EffectParameter _lightOffsetParam;

        private EffectParameter _ambientColorParam;

        private EffectParameter _lightDirectionParam;

        private EffectParameter _cameraPositionParam;

        private EffectParameter _cameraFarDistanceParam;
        
        private EffectParameter _lightViewParam;
        private EffectParameter _lightProjectionParam;

        private EffectParameter _frameParam;
        //EffectParameter worldViewProjParam;

        #endregion

        #region Fields

        private bool _fogEnabled;
        private bool _vertexColorEnabled;

        private Matrix _world      = Matrix.Identity;
        private Matrix _view       = Matrix.Identity;
        private Matrix _projection = Matrix.Identity;

        private Matrix _lightView       = Matrix.Identity;
        private Matrix _lightProjection = Matrix.Identity;
        
        private Matrix _worldView;

        private Vector3 _diffuseColor = Vector3.One;

        private float _alpha = 1;

        private float _fogStart = 0;
        private float _fogEnd   = 1;

        private CompareFunction _alphaFunction = CompareFunction.Greater;
        private int             _referenceAlpha;
        private bool            _isEqNe;

        private EffectDirtyFlags _dirtyFlags = EffectDirtyFlags.All;

        private Color _ambientColor = Color.White;
        private Vector3 _lightDirection = Vector3.Forward;
        
        private Vector3 _cameraPosition =  Vector3.Zero;
        private float _cameraFarDistance = 0;

        private float _frame = 0;
        private bool _applyAnimations = false;
        #endregion

        #region Public Properties

        public bool ApplyAnimations
        {
            get
            {
                return _applyAnimations;
            }
            set
            {
                _applyAnimations = value;
                _dirtyFlags |= EffectDirtyFlags.Frame;
            }
        }

        public float Frame
        {
            get
            {
                return _frame;
            }
            set
            {
                _frame = value;
                _dirtyFlags |= EffectDirtyFlags.Frame;
            }
        }
        
        public float CameraFarDistance
        {
            get
            {
                return _cameraFarDistance;
            }
            set
            {
                _cameraFarDistance = value;
                _dirtyFlags |= EffectDirtyFlags.CameraFarDistance;
            }
        }
        
        public Vector3 CameraPosition
        {
            get
            {
                return _cameraPosition;
            }
            set
            {
                _cameraPosition = value;
                _dirtyFlags |= EffectDirtyFlags.CameraPosition;
            }
        }

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
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix LightProjection
        {
            get { return _lightProjection; }

            set
            {
                _lightProjection = value;
                _dirtyFlags |= EffectDirtyFlags.LightViewProjection;
            }
        }

        
        public Matrix LightView
        {
            get { return _lightView; }

            set
            {
                _lightView = value;
                _dirtyFlags |= EffectDirtyFlags.LightViewProjection;
            }
        }

        /// <summary>
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return _diffuseColor; }

            set
            {
                _diffuseColor = value;
                _dirtyFlags |= EffectDirtyFlags.MaterialColor;
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


        /// <summary>
        /// Gets or sets the fog enable flag.
        /// </summary>
        public bool FogEnabled
        {
            get { return _fogEnabled; }

            set
            {
                if (_fogEnabled != value)
                {
                    _fogEnabled = value;
                    _dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
                }
            }
        }


        /// <summary>
        /// Gets or sets the fog start distance.
        /// </summary>
        public float FogStart
        {
            get { return _fogStart; }

            set
            {
                _fogStart = value;
                _dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog end distance.
        /// </summary>
        public float FogEnd
        {
            get { return _fogEnd; }

            set
            {
                _fogEnd = value;
                _dirtyFlags |= EffectDirtyFlags.Fog;
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
        /// Gets or sets the ambient light color
        /// </summary>
        public Color AmbientLightColor
        {
            get { return _ambientColor; }

            set
            {
                _ambientColor = value;
                _dirtyFlags |= EffectDirtyFlags.AmbientColor;
            }
        }
        
        /// <summary>
        /// Gets or sets the ambient light color
        /// </summary>
        public Vector3 AmbientLightDirection
        {
            get { return _lightDirection; }

            set
            {
                _lightDirection = value;
                _dirtyFlags |= EffectDirtyFlags.LightDirection;
            }
        }
        
        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
      /*  public Vector3 FogColor
        {
            get { return fogColorParam.GetValueVector3(); }
            set { fogColorParam.SetValue(value); }
        }*/
      
        public Vector3 FogColor { get; set; }


        /// <summary>
        /// Gets or sets the current texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return _textureParam.GetValueTexture2D(); }
            set
            {
                _textureParam.SetValue(value);
                _textureScaleParam.SetValue(Vector2.One / value.Bounds.Size.ToVector2());
            }
        }


        /// <summary>
        /// Gets or sets whether vertex color is enabled.
        /// </summary>
        public bool VertexColorEnabled
        {
            get { return _vertexColorEnabled; }

            set
            {
                if (_vertexColorEnabled != value)
                {
                    _vertexColorEnabled = value;
                    _dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
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
        public BlockEffect(GraphicsDevice device, byte[] byteCode) : base(device, byteCode)
        {
            CacheEffectParameters();
        }
        
        public BlockEffect() : base(ResourceManager.BlockEffect)
        {
            CacheEffectParameters();
        }
        
        /// <summary>
        /// Creates a new AlphaTestEffect by cloning parameter settings from an existing instance.
        /// </summary>
        public BlockEffect(BlockEffect cloneSource) : base(cloneSource)
        {
            CacheEffectParameters();

            _fogEnabled = cloneSource._fogEnabled;
            _vertexColorEnabled = cloneSource._vertexColorEnabled;

            _world = cloneSource._world;
            _view = cloneSource._view;
            _projection = cloneSource._projection;

            _diffuseColor = cloneSource._diffuseColor;

            _alpha = cloneSource._alpha;

            _fogStart = cloneSource._fogStart;
            _fogEnd = cloneSource._fogEnd;

            _alphaFunction = cloneSource._alphaFunction;
            _referenceAlpha = cloneSource._referenceAlpha;

            _lightOffset = cloneSource._lightOffset;

            _lightDirection = cloneSource._lightDirection;
            _ambientColor = cloneSource._ambientColor;
            _cameraPosition = cloneSource._cameraPosition;
            _cameraFarDistance = cloneSource._cameraFarDistance;

            _lightView = cloneSource._lightView;
            _lightProjection = cloneSource._lightProjection;

            _applyAnimations = cloneSource._applyAnimations;
            //  _lightSource1 = cloneSource._lightSource1;
            // _lightSource1Strength = cloneSource._lightSource1Strength;
        }

        /// <summary>
        /// Creates a clone of the current AlphaTestEffect instance.
        /// </summary>
        public override Microsoft.Xna.Framework.Graphics.Effect Clone()
        {
            return new BlockEffect(this);
        }

        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            _textureParam = Parameters["Texture"];
           // diffuseColorParam = Parameters["DiffuseColor"];
            _alphaTestParam = Parameters["AlphaTest"];
            _fogColorParam = Parameters["FogColor"];
            _fogStartParam = Parameters["FogStart"];
            _fogEndParam = Parameters["FogEnd"];
            _fogEnabledParam = Parameters["FogEnabled"];
          //  fogVectorParam = Parameters["FogVector"];
        
            _worldParam = Parameters["World"];

            _projParam = Parameters["Projection"];

            _viewParam = Parameters["View"];

            _lightOffsetParam = Parameters["LightOffset"];
            _ambientColorParam = Parameters["AmbientColor"];
            _lightDirectionParam = Parameters["LightDirection"];
           // _cameraPositionParam = Parameters["CameraPosition"];
            _cameraFarDistanceParam = Parameters["CameraFarDistance"];

            _lightViewParam = Parameters["LightView"];
            _lightProjectionParam = Parameters["LightProjection"];
            _frameParam = Parameters["ElapsedTime"];
            _textureScaleParam = Parameters["UvScale"];
            _applyAnimationsParameter = Parameters["ApplyAnimations"];
            //  worldViewProjParam = Parameters["WorldViewProj"];
        }

        internal static EffectDirtyFlags SetWorldViewProjAndFog(
            EffectDirtyFlags dirtyFlags,
            ref Matrix world,
            ref Matrix view,
            ref Matrix projection,
            ref Matrix worldView,
            bool fogEnabled,
            float fogStart,
            float fogEnd,
            EffectParameter worldParam,
            EffectParameter viewParam,
            EffectParameter projectionParam,
            EffectParameter fogStartParam,
            EffectParameter fogEndParam,
            EffectParameter fogEnabledParam,
            EffectParameter fogColorParam,
            Vector3 fogColor)
        {
            if ((dirtyFlags & EffectDirtyFlags.WorldViewProj) != ~EffectDirtyFlags.All)
            {
                worldParam.SetValue(world);
                viewParam.SetValue(view);
                projectionParam.SetValue(projection);
                dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
            }
            if (fogEnabled)
            {
                if ((dirtyFlags & (EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable)) != ~EffectDirtyFlags.All)
                {
                   fogStartParam.SetValue(fogStart);
                   fogEndParam.SetValue(fogEnd);
                   fogEnabledParam.SetValue(1f);
                   fogColorParam.SetValue(fogColor);
                    dirtyFlags &= ~(EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable);
                }
            }
            else if ((dirtyFlags & EffectDirtyFlags.FogEnable) != ~EffectDirtyFlags.All)
            {
                fogEnabledParam.SetValue(0f);
                dirtyFlags &= ~EffectDirtyFlags.FogEnable;
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
                _dirtyFlags, ref _world, ref _view, ref _projection, ref _worldView, _fogEnabled, _fogStart, _fogEnd,
                _worldParam, _viewParam, _projParam, _fogStartParam, _fogEndParam, _fogEnabledParam, _fogColorParam, FogColor);

                        
            if ((_dirtyFlags & EffectDirtyFlags.LightViewProjection) != 0)
            {
                _lightViewParam.SetValue(_lightView);
                //_lightProjectionParam.SetValue(_lightProjection);
                _dirtyFlags &= ~EffectDirtyFlags.LightViewProjection;
            }
            
            
            if ((_dirtyFlags & EffectDirtyFlags.CameraFarDistance) != 0)
            {
                _cameraFarDistanceParam.SetValue(_cameraFarDistance);
                _dirtyFlags &= ~EffectDirtyFlags.CameraFarDistance;
            }
            
            if ((_dirtyFlags & EffectDirtyFlags.CameraPosition) != 0)
            {
               // _cameraPositionParam.SetValue(_cameraPosition);
                _dirtyFlags &= ~EffectDirtyFlags.CameraPosition;
            }
            
            if ((_dirtyFlags & EffectDirtyFlags.LightDirection) != 0)
            {
            //  _lightDirectionParam.SetValue(_lightDirection);
                _dirtyFlags &= ~EffectDirtyFlags.LightDirection;
            }
            
            if ((_dirtyFlags & EffectDirtyFlags.AmbientColor) != 0)
            {
               // _ambientColorParam.SetValue(_ambientColor.ToVector3());
                _dirtyFlags &= ~EffectDirtyFlags.AmbientColor;
            }
            
            if ((_dirtyFlags & EffectDirtyFlags.LightOffset) != 0)
            {
                _lightOffsetParam.SetValue((float)_lightOffset);
                _dirtyFlags &= ~EffectDirtyFlags.LightOffset;
            }
            
            // Recompute the diffuse/alpha material color parameter?
            if ((_dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
              //  diffuseColorParam.SetValue(new Vector4(diffuseColor * alpha, alpha));

                _dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            // Recompute the alpha test settings?
            if ((_dirtyFlags & EffectDirtyFlags.AlphaTest) != 0)
            {
                Vector4 alphaTest = new Vector4();
                bool    eqNe      = false;

                // Convert reference alpha from 8 bit integer to 0-1 float format.
                float reference = (float) _referenceAlpha / 255f;

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
                    _dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
                }
            }

            if ((_dirtyFlags & EffectDirtyFlags.Frame) != 0)
            {
                _applyAnimationsParameter.SetValue(_applyAnimations ? 1f : 0f);
                _frameParam.SetValue(_frame);
                _dirtyFlags &= ~EffectDirtyFlags.Frame;
            }
            
            // Recompute the shader index?
            if ((_dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                CurrentTechnique = Techniques[0];
            }
        }


        #endregion
    }
}