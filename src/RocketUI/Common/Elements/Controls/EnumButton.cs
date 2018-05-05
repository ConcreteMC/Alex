using System;

namespace RocketUI.Elements.Controls
{
    public class EnumButton<TEnum> : Button, IValuedControl<TEnum> where TEnum : struct
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
                    
                    TextBlock.Text = string.Format(DisplayFormat, _value);

                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public string DisplayFormat { get; set; } = "{0}";
    }
}
