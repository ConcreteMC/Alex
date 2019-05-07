using System;

namespace Alex.API.Data.Options
{

    public class OptionsPropertyAccessor<TProperty> : IDisposable
    {
        private readonly OptionsProperty<TProperty> _property;

        private readonly OptionsPropertyChangedDelegate<TProperty> _delegate;

        internal OptionsPropertyAccessor(OptionsProperty<TProperty> property, OptionsPropertyChangedDelegate<TProperty> listenDelegate)
        {
            _property = property;
            _delegate = listenDelegate;
        }

        internal void Invoke(TProperty oldValue, TProperty newValue)
        {
            _delegate?.Invoke(oldValue, newValue);
        }

        public void Dispose()
        {
            _property.Unbind(this);
        }
    }
}