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
        LightSource1  = 512,
        All           = -1, // 0xFFFFFFFF
    }

    public class BlockEffect : Microsoft.Xna.Framework.Graphics.Effect, IEffectMatrices, IEffectFog
    {
        #region Effect Parameters

        EffectParameter textureParam;
       // EffectParameter diffuseColorParam;
        EffectParameter alphaTestParam;
        EffectParameter fogColorParam;
        private EffectParameter fogStartParam;
        private EffectParameter fogEndParam;
        private EffectParameter fogEnabledParam;
        
        EffectParameter fogVectorParam;

        private EffectParameter worldParam;

        private EffectParameter projParam;

        private EffectParameter viewParam;

        private EffectParameter lightOffsetParam;

        private EffectParameter lightSource1StrengthParam;

        private EffectParameter lightSource1Param;
         //EffectParameter worldViewProjParam;

        #endregion

        #region Fields

        private Vector3 lightSource1;
        private float lightSource1Strength;
        
        bool fogEnabled;
        bool vertexColorEnabled;

        Matrix world      = Matrix.Identity;
        Matrix view       = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;

        float alpha = 1;

        float fogStart = 0;
        float fogEnd   = 1;

        CompareFunction alphaFunction = CompareFunction.Greater;
        int             referenceAlpha;
        bool            isEqNe;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        #endregion

        #region Public Properties

        public float LightSource1Strength
        {
            get => lightSource1Strength;
            set
            {
                lightSource1Strength = value;
                dirtyFlags |= EffectDirtyFlags.LightSource1;
            }
        }

        public Vector3 LightSource1
        {
            get
            {
                return lightSource1;
            }
            set
            {
                lightSource1 = value;
                dirtyFlags |= EffectDirtyFlags.LightSource1;
            }
        }

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
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }

            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
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


        /// <summary>
        /// Gets or sets the fog enable flag.
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }

            set
            {
                if (fogEnabled != value)
                {
                    fogEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
                }
            }
        }


        /// <summary>
        /// Gets or sets the fog start distance.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }

            set
            {
                fogStart = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog end distance.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }

            set
            {
                fogEnd = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
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
            get { return textureParam.GetValueTexture2D(); }
            set { textureParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets whether vertex color is enabled.
        /// </summary>
        public bool VertexColorEnabled
        {
            get { return vertexColorEnabled; }

            set
            {
                if (vertexColorEnabled != value)
                {
                    vertexColorEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
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

            fogEnabled = cloneSource.fogEnabled;
            vertexColorEnabled = cloneSource.vertexColorEnabled;

            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            diffuseColor = cloneSource.diffuseColor;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;

            alphaFunction = cloneSource.alphaFunction;
            referenceAlpha = cloneSource.referenceAlpha;

            lightSource1 = cloneSource.lightSource1;
            lightSource1Strength = cloneSource.lightSource1Strength;
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
            textureParam = Parameters["Texture"];
           // diffuseColorParam = Parameters["DiffuseColor"];
            alphaTestParam = Parameters["AlphaTest"];
            fogColorParam = Parameters["FogColor"];
            fogStartParam = Parameters["FogStart"];
            fogEndParam = Parameters["FogEnd"];
            fogEnabledParam = Parameters["FogEnabled"];
          //  fogVectorParam = Parameters["FogVector"];
        
            worldParam = Parameters["World"];

            projParam = Parameters["Projection"];

            viewParam = Parameters["View"];

            lightOffsetParam = Parameters["LightOffset"];

            lightSource1Param = Parameters["LightSource1"];
            lightSource1StrengthParam = Parameters["LightSource1Strength"];
            //  worldViewProjParam = Parameters["WorldViewProj"];
        }
        
        private static void SetFogVector(
            ref Matrix worldView,
            float fogStart,
            float fogEnd,
            EffectParameter fogVectorParam)
        {
            if ((double) fogStart == (double) fogEnd)
            {
                fogVectorParam.SetValue(new Vector4(0.0f, 0.0f, 0.0f, 1f));
            }
            else
            {
                float num = (float) (1.0 / ((double) fogStart - (double) fogEnd));
                fogVectorParam.SetValue(new Vector4()
                {
                    X = worldView.M13 * num,
                    Y = worldView.M23 * num,
                    Z = worldView.M33 * num,
                    W = (worldView.M43 + fogStart) * num
                });
            }
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
             //   Matrix.Multiply(ref world, ref view, out worldView);
            //    Matrix result;
             //   Matrix.Multiply(ref worldView, ref projection, out result);
               // worldViewProjParam.SetValue(result);
                worldParam.SetValue(world);
                viewParam.SetValue(view);
                projectionParam.SetValue(projection);
                dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
            }
            if (fogEnabled)
            {
                if ((dirtyFlags & (EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable)) != ~EffectDirtyFlags.All)
                {
                   // SetFogVector(ref worldView, fogStart, fogEnd, fogVectorParam);
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
                //fogVectorParam.SetValue(Vector4.Zero);
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
            dirtyFlags = SetWorldViewProjAndFog(
                dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd,
                worldParam, viewParam, projParam, fogStartParam, fogEndParam, fogEnabledParam, fogColorParam, FogColor);

            if ((dirtyFlags & EffectDirtyFlags.LightSource1) != 0)
            {
                //lightSource1Param.SetValue(lightSource1);
              //  lightSource1StrengthParam.SetValue(lightSource1Strength);
                
                dirtyFlags &= ~EffectDirtyFlags.LightSource1;
            }
            
            if ((dirtyFlags & EffectDirtyFlags.LightOffset) != 0)
            {
                lightOffsetParam.SetValue((float)_lightOffset);
                dirtyFlags &= ~EffectDirtyFlags.LightOffset;
            }
            
            // Recompute the diffuse/alpha material color parameter?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
              //  diffuseColorParam.SetValue(new Vector4(diffuseColor * alpha, alpha));

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
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
                CurrentTechnique = Techniques[0];
            }
        }


        #endregion
    }
}