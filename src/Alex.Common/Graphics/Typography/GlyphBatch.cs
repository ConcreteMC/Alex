using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common.Graphics.Typography
{
    public class GlyphBatch
    {
        private readonly GraphicsDevice              _device;
        private readonly Matrix                      _projection;
        private          Effect                      _currentEffect;
        private readonly List<VertexPositionTexture> _verts = new List<VertexPositionTexture>();


        public GlyphBatch(GraphicsDevice device)
        {
            _device = device;

            _projection =
                Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, 0.1f, 1);
        }

        public void Start(Texture2D texture, Effect effect, Color color)
        {
            _currentEffect = effect;
            var stat = new RasterizerState();
            stat.CullMode = CullMode.None;

            _device.BlendState = BlendState.NonPremultiplied;
            _device.DepthStencilState = DepthStencilState.Default;
            var ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;
            ss.Filter = TextureFilter.Anisotropic;

            _currentEffect.Parameters["colour"].SetValue(color.ToVector4());
            _currentEffect.Parameters["Texture"].SetValue(texture);
            _currentEffect.Parameters["Projection"].SetValue(_projection);
            _device.SamplerStates[0] = ss;
            _device.RasterizerState = stat;
        }

        public void StartSprite(Texture2D texture, Effect effect, Color color)
        {
            _currentEffect = effect;
            var stat = new RasterizerState();
            stat.CullMode = CullMode.None;

            _device.BlendState = BlendState.NonPremultiplied;
            _device.DepthStencilState = DepthStencilState.Default;
            var ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Wrap;
            ss.AddressV = TextureAddressMode.Wrap;
            ss.Filter = TextureFilter.Anisotropic;

            _currentEffect.Parameters["colour"].SetValue(color.ToVector4());
            _currentEffect.Parameters["Texture"].SetValue(texture);
            _currentEffect.Parameters["Projection"].SetValue(_projection);
            _device.SamplerStates[0] = ss;
            _device.RasterizerState = stat;
        }

        public void Draw(Vector4 src, Vector4 dst)
        {
            var v1 = new VertexPositionTexture
            {
                Position = new Vector3(dst.X, dst.Y, -0.5f),
                TextureCoordinate = new Vector2(src.X, src.Y)
            };
            _verts.Add(v1);

            v1 = new VertexPositionTexture
            {
                Position = new Vector3(dst.X + dst.Z, dst.Y, -0.5f),
                TextureCoordinate = new Vector2(src.X + src.Z, src.Y)
            };
            _verts.Add(v1);

            v1 = new VertexPositionTexture
            {
                Position = new Vector3(dst.X, dst.Y + dst.W, -0.5f),
                TextureCoordinate = new Vector2(src.X, src.Y + src.W)
            };
            _verts.Add(v1);

            v1 = new VertexPositionTexture
            {
                Position = new Vector3(dst.X + dst.Z, dst.Y, -0.5f),
                TextureCoordinate = new Vector2(src.X + src.Z, src.Y)
            };
            _verts.Add(v1);

            v1 = new VertexPositionTexture
            {
                Position = new Vector3(dst.X + dst.Z, dst.Y + dst.W, -0.5f),
                TextureCoordinate = new Vector2(src.X + src.Z, src.Y + src.W)
            };
            _verts.Add(v1);

            v1 = new VertexPositionTexture
            {
                Position = new Vector3(dst.X, dst.Y + dst.W, -0.5f),
                TextureCoordinate = new Vector2(src.X, src.Y + src.W)
            };
            _verts.Add(v1);
        }

        public void End()
        {
            if (_verts.Count == 0)
                return;

            var lverts = _verts.ToArray();
            var         stat   = new RasterizerState();
            stat.CullMode = CullMode.None;
            _device.BlendState = BlendState.NonPremultiplied;
            _device.DepthStencilState = DepthStencilState.None;

            foreach (var pass in _currentEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, lverts, 0,
                    _verts.Count / 3);
            }

            _verts.Clear();
        }
    }
}