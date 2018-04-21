using System;

namespace Alex.API.Data.Options
{
    public class OptionsPropertyChangedEventArgs<TProperty> : EventArgs
    {
        public OptionsProperty<TProperty> OptionsProperty { get; }
        public TProperty                  OldValue        { get; }
        public TProperty                  NewValue        { get; }

        internal OptionsPropertyChangedEventArgs(OptionsProperty<TProperty> property, TProperty oldValue,
                                                 TProperty                  newValue)
        {
            OptionsProperty = property;
            OldValue        = oldValue;
            NewValue        = newValue;
        }
    }
}