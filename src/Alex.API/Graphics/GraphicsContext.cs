using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{
    public class GraphicsContext : IDisposable
    {
        public EventHandler Disposed;

        public GraphicsDevice GraphicsDevice { get; }

        #region Properties

        private readonly PropertyState<GraphicsDevice, Viewport> _viewportProperty;
        private readonly PropertyState<GraphicsDevice, BlendState> _blendStateProperty;
        private readonly PropertyState<GraphicsDevice, DepthStencilState> _depthStencilStateProperty;
        private readonly PropertyState<GraphicsDevice, RasterizerState> _rasterizerStateProperty;
        private readonly PropertyState<GraphicsDevice, SamplerState> _samplerStateProperty;
        private readonly PropertyState<GraphicsDevice, Rectangle> _scissorRectangleProperty;
        private readonly PropertyState<GraphicsDevice, Color> _blendFactorProperty;

        public Viewport Viewport
        {
            get => _viewportProperty;
            set => _viewportProperty.Set(value);
        }
        public BlendState BlendState
        {
            get => _blendStateProperty;
            set => _blendStateProperty.Set(value);
        }
        public DepthStencilState DepthStencilState
        {
            get => _depthStencilStateProperty;
            set => _depthStencilStateProperty.Set(value);
        }
        public RasterizerState RasterizerState
        {
            get => _rasterizerStateProperty;
            set => _rasterizerStateProperty.Set(value);
        }
        public SamplerState SamplerState
        {
            get => _samplerStateProperty;
            set => _samplerStateProperty.Set(value);
        }
        public Rectangle ScissorRectangle
        {
            get => _scissorRectangleProperty;
            set => _scissorRectangleProperty.Set(value);
        }
        public Color BlendFactor
        {
            get => _blendFactorProperty;
            set => _blendFactorProperty.Set(value);
        }

        #endregion

        public GraphicsContext(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            _viewportProperty           = CreateState(g => g.Viewport,              (g, v) => g.Viewport = v);
            _blendStateProperty         = CreateState(g => g.BlendState,            (g, v) => g.BlendState = v);
            _depthStencilStateProperty  = CreateState(g => g.DepthStencilState,     (g, v) => g.DepthStencilState = v);
            _rasterizerStateProperty    = CreateState(g => g.RasterizerState,       (g, v) => g.RasterizerState = v);
            _samplerStateProperty       = CreateState(g => g.SamplerStates[0],      (g, v) => g.SamplerStates[0] = v);
            _scissorRectangleProperty   = CreateState(g => g.ScissorRectangle,      (g, v) => g.ScissorRectangle = v);
            _blendFactorProperty        = CreateState(g => g.BlendFactor,           (g, v) => g.BlendFactor = v);
        }

        public GraphicsContext CreateContext(BlendState blendState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, SamplerState samplerState = null)
        {
            return CreateContext(GraphicsDevice, blendState, depthStencilState, rasterizerState, samplerState);
        }

        public void RestoreState()
        {
            ForEachProperty(p => p.RestoreInitialValue());
        }

        private void ForEachProperty(Action<IPropertyState<GraphicsDevice>> action)
        {
            if (action == null) return;
            
            action.Invoke(_viewportProperty);
            action.Invoke(_blendStateProperty);
            action.Invoke(_depthStencilStateProperty);
            action.Invoke(_rasterizerStateProperty);
            action.Invoke(_samplerStateProperty);
            action.Invoke(_scissorRectangleProperty);
            action.Invoke(_blendFactorProperty);
        }

        public static GraphicsContext CreateContext(GraphicsDevice    graphicsDevice, 
                                                    BlendState blendState = null,
                                                    DepthStencilState depthStencilState = null,
                                                    RasterizerState   rasterizerState   = null,
                                                    SamplerState      samplerState      = null)
        {
            var context = new GraphicsContext(graphicsDevice);

            if (blendState != null)
            {
                context.BlendState = blendState;
            }

            if (depthStencilState != null)
            {
                context.DepthStencilState = depthStencilState;
            }

            if (rasterizerState != null)
            {
                context.RasterizerState = rasterizerState;
            }

            if (samplerState != null)
            {
                context.SamplerState = samplerState;
            }

            return context;
        }

        #region Dispose
        
        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    ForEachProperty(p => p.Dispose());

                    Disposed?.Invoke(this, null);
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        private PropertyState<GraphicsDevice, TPropertyType> CreateState<TPropertyType>(Func<GraphicsDevice, TPropertyType> getProperty, Action<GraphicsDevice, TPropertyType> setProperty)
        {
            return new PropertyState<GraphicsDevice, TPropertyType>(GraphicsDevice, getProperty(GraphicsDevice), setProperty);
        }

        public interface IPropertyState<TPropertyOwner> : IDisposable
        {
            void RestoreInitialValue();
        }

        public class PropertyState<TPropertyOwner, TPropertyType> : IPropertyState<TPropertyOwner>
        {
            public TPropertyType InitialValue { get; }
            public TPropertyType Value
            {
                get => _currentValue;
                set => Set(value);
            }

            private bool _dirty;
            private TPropertyType _currentValue;
            private readonly TPropertyOwner _owner;

            private Func<TPropertyOwner, TPropertyType> _getValueFunc;
            private readonly Action<TPropertyOwner, TPropertyType> _setValueFunc;


            public PropertyState(TPropertyOwner owner, TPropertyType initialValue, Action<TPropertyOwner, TPropertyType> setValueFunc)
            {
                _owner = owner;
                _setValueFunc = setValueFunc;
                
                InitialValue = initialValue;
                _currentValue = initialValue;
            }

            public TPropertyType Set(TPropertyType newValue)
            { 
                _dirty          = true;
                _currentValue   = newValue;

                _setValueFunc(_owner, _currentValue);

                return Value;
            }

            public void RestoreInitialValue()
            {
                if (_dirty)
                {
                    _setValueFunc(_owner, InitialValue);
                }
            }

            public void Dispose()
            {
                RestoreInitialValue();
            }

            public static implicit operator TPropertyType(PropertyState<TPropertyOwner, TPropertyType> propertyState)
            {
                return propertyState.Value;
            }
        }


    }
}
