using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Effect
{
    public class ShadowMapEffect
    {
        private readonly Microsoft.Xna.Framework.Graphics.Effect _innerEffect;

        private readonly EffectParameter _worldViewProjectionParameter;

        public Matrix WorldViewProjection { get; set; }

        public ShadowMapEffect(GraphicsDevice graphicsDevice, Microsoft.Xna.Framework.Graphics.Effect innerEffect) 
        {
            _innerEffect = innerEffect;

            _worldViewProjectionParameter = _innerEffect.Parameters["WorldViewProjection"];
        }

        public void Apply()
        {
            _worldViewProjectionParameter.SetValue(WorldViewProjection);

            _innerEffect.CurrentTechnique.Passes[0].Apply();
        }
    }
}