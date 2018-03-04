using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Alex.Gui.Themes
{
    public interface IUiElementStyleProperty
    {
        int Priority { get; }

        object Value { get; set; }

        bool HasValue { get; set; }
    }

    public struct UiElementStyleProperty<TProperty> : IUiElementStyleProperty
    {
        public int Priority { get; }

        object IUiElementStyleProperty.Value
        {
            get => Value;
            set => Value = (TProperty) value;
        }

        public TProperty Value { get; set; }

        public bool HasValue { get; set; }
        
        public UiElementStyleProperty(TProperty value, int priority = -1)
        {
            Value = value;
            Priority = priority;
            HasValue = value != null;
        }

        public static implicit operator UiElementStyleProperty<TProperty>(TProperty value)
        {
            return new UiElementStyleProperty<TProperty>(value);
        }

        public static implicit operator TProperty(UiElementStyleProperty<TProperty> value)
        {
            return value.Value;
        }
    }
}
