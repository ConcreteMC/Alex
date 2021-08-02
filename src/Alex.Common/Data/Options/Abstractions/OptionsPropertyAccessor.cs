using System;

namespace Alex.Common.Data.Options
{

    public class OptionsPropertyAccessor<TProperty> : IDisposable
    {
        private readonly OptionsProperty<TProperty> _property;

        private readonly OptionsPropertyChangedDelegate<TProperty> _delegate;

        public TProperty Value => _property.Value;
        internal OptionsPropertyAccessor(OptionsProperty<TProperty> property, OptionsPropertyChangedDelegate<TProperty> listenDelegate)
        {
            _property = property;
            _delegate = listenDelegate;
        }

        internal void Invoke(TProperty oldValue, TProperty newValue)
        {
            _delegate?.Invoke(oldValue, newValue);
        }
        
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _property.Unbind(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~OptionsPropertyAccessor()
        {
            Dispose(false);
        }
    }
}