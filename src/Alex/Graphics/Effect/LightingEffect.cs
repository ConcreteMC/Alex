using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Effect
{
    public class LightingEffect : Microsoft.Xna.Framework.Graphics.Effect, IEffectMatrices
    {
        #region Effect Parameters

        EffectParameter textureParam;
       // EffectParameter diffuseColorParam;
        EffectParameter alphaTestParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;

        private EffectParameter worldParam;

        private EffectParameter projParam;

        private EffectParameter viewParam;

        private EffectParameter lightOffsetParam;
         //EffectParameter worldViewProjParam;

        #endregion

        #region Fields

        Matrix world      = Matrix.Identity;
        Matrix view       = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        float alpha = 1;

        float fogStart = 0;
        float fogEnd   = 1;

        CompareFunction alphaFunction = CompareFunction.Greater;
        int             referenceAlpha;
        bool            isEqNe;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        #endregion

        #region Public Properties


        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }

            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }

            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }

            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }


        /// <summary>
        /// Gets or sets the material alpha.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }

            set
            {
                alpha = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
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
                dirtyFlags |= EffectDirtyFlags.LightOffset;
            }
        }

        /// <summary>
        /// Gets or sets the alpha compare function (default Greater).
        /// </summary>
        public CompareFunction AlphaFunction
        {
            get { return alphaFunction; }

            set
            {
                alphaFunction = value;
                dirtyFlags |= EffectDirtyFlags.AlphaTest;
            }
        }


        /// <summary>
        /// Gets or sets the reference alpha value (default 0).
        /// </summary>
        public int ReferenceAlpha
        {
            get { return referenceAlpha; }

            set
            {
                referenceAlpha = value;
                dirtyFlags |= EffectDirtyFlags.AlphaTest;
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
            
            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;

            alphaFunction = cloneSource.alphaFunction;
            referenceAlpha = cloneSource.referenceAlpha;
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
        void CacheEffectParameters()
        {
            alphaTestParam = Parameters["AlphaTest"];

            worldParam = Parameters["World"];

            projParam = Parameters["Projection"];

            viewParam = Parameters["View"];

            lightOffsetParam = Parameters["LightOffset"];
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
            dirtyFlags = SetWorldViewProjAndFog(
                dirtyFlags, ref world, ref view, ref projection, ref worldView,
                worldParam, viewParam, projParam);

            if ((dirtyFlags & EffectDirtyFlags.LightOffset) != 0)
            {
                lightOffsetParam.SetValue((float)_lightOffset);
                dirtyFlags &= ~EffectDirtyFlags.LightOffset;
            }

            // Recompute the alpha test settings?
            if ((dirtyFlags & EffectDirtyFlags.AlphaTest) != 0)
            {
                Vector4 alphaTest = new Vector4();
                bool    eqNe      = false;

                // Convert reference alpha from 8 bit integer to 0-1 float format.
                float reference = (float) referenceAlpha / 255f;

                // Comparison tolerance of half the 8 bit integer precision.
                const float threshold = 0.5f / 255f;

                switch (alphaFunction)
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

                alphaTestParam.SetValue(alphaTest);

                dirtyFlags &= ~EffectDirtyFlags.AlphaTest;

                // If we changed between less/greater vs. equal/notequal
                // compare modes, we must also update the shader index.
                if (isEqNe != eqNe)
                {
                    isEqNe = eqNe;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                CurrentTechnique = Techniques[0];
            }
        }


        #endregion
    }
}