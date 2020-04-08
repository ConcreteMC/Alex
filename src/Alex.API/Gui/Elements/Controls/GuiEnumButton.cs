using System;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiEnumButton<TEnum> : GuiButton, IValuedControl<TEnum> where TEnum : struct
    {
        public event EventHandler<TEnum> ValueChanged;
        private TEnum _value;

        public TEnum Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    
                    TextElement.Text = DisplayFormat?.FormatValue(_value) ?? string.Empty;

                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public ValueFormatter<TEnum> DisplayFormat { get; set; } = "{0}";
    }
}
