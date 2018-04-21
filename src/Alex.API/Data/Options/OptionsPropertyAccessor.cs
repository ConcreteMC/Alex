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
        }

        internal void Invoke(OptionsPropertyChangedEventArgs<TProperty> args)
        {

        }

        public void Dispose()
        {
            //_property.Unbind(this);
        }
    }
}