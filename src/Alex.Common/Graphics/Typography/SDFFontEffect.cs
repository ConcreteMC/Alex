using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common.Graphics.Typography
{
    public class SDFFontEffect : Effect
    {
        private EffectParameter _textureParam;
        private EffectParameter _colourParam;
        private EffectParameter _projectionParam;

        public Texture2D Texture2D
        {
            get => _textureParam.GetValueTexture2D();
            set => _textureParam.SetValue(value);
        }
        
        public Color Color
        {
            get => new Color(_colourParam.GetValueVector4());
            set => _colourParam.SetValue(value.ToVector4());
        }
        
        public Matrix Projection
        {
            get => _projectionParam.GetValueMatrix();
            set => _projectionParam.SetValue(value);
        }
        
        protected SDFFontEffect(Effect cloneSource) : base(cloneSource)
        {
            CacheEffectParameters();
        }

        public SDFFontEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            CacheEffectParameters();
        }

        public SDFFontEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
            CacheEffectParameters();
        }

        private void CacheEffectParameters()
        {
            _textureParam = Parameters["Texture"];
            _projectionParam = Parameters["Projection"];
            _colourParam = Parameters["colour"];
        }
    }
}