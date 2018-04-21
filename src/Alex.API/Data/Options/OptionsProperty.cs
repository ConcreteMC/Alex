using System;

namespace Alex.API.Data.Options
{
    public class OptionsProperty<TProperty> : IOptionsProperty
    {
        public event EventHandler<OptionsPropertyChangedEventArgs<TProperty>> ValueChanged;

        private TProperty _value;
        public TProperty Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                var newValue = value;

                _value = newValue;
                ValueChanged?.Invoke(this, new OptionsPropertyChangedEventArgs<TProperty>(this, oldValue, newValue));
            }
        }


        private readonly OptionsPropertyValidator<TProperty> _validator;

        internal OptionsProperty(TProperty defaultValue, OptionsPropertyValidator<TProperty> validator = null)
        {
            _validator = validator;
        }

        //public OptionsPropertyAccessor<TProperty> Bind(OptionsPropertyChangedDelegate<TProperty> listenerDelegate)
        //{
        //    var accessor = new OptionsPropertyAccessor<TProperty>(this, listenerDelegate);

        //}

        //internal void Unbind(OptionsPropertyAccessor<TProperty> accessor)
        //{

        //}
    }
}