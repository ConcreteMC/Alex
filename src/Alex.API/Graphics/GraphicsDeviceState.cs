using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{
    public class GraphicsDeviceState : IDisposable
    {
        public GraphicsDevice Graphics { get; }
        
        public GraphicsDeviceState PreviousState {get;}

        private DepthStencilState _depthStencilState;
        private SamplerState _samplerState;
        private RasterizerState _rasterizerState;
        private BlendState _blendState;

        public DepthStencilState DepthStencilState
        {
            get => _depthStencilState;
            set
            {
                _depthStencilState = value;
                Graphics.DepthStencilState = _depthStencilState;
            }
        }

        public SamplerState SamplerState
        {
            get => _samplerState;
            set
            {
                _samplerState = value;
                Graphics.SamplerStates[0] = _samplerState;
            }
        }

        public RasterizerState RasterizerState
        {
            get => _rasterizerState;
            set
            {
                _rasterizerState = value;
                Graphics.RasterizerState = _rasterizerState;
            }
        }

        public BlendState BlendState
        {
            get => _blendState;
            set
            {
                _blendState = value;
                Graphics.BlendState = _blendState;
            }
        }

        public GraphicsDeviceState(GraphicsDevice graphics, GraphicsDeviceState previousState)
        {
            PreviousState = previousState;
            Graphics = graphics;


        }

        public void Dispose()
        {

        }
    }
}
