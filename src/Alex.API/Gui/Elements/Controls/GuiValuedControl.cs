using System;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiValuedControl<TValue> : GuiControl, IValuedControl<TValue> where TValue : IEquatable<TValue>
    {
        public event EventHandler<TValue> ValueChanged;

	    public GuiValuedControl()
	    {
		    _value = default(TValue);
	    }

        private TValue _value;
        public TValue Value
        {
            get => _value;
            set
            {
                if (!Equals(value, _value) || _value == null)
                {
                    if (OnValueChanged(value))
                    {
                        _value = value;
                        ValueChanged?.Invoke(this, _value);
                    }
                }
            }
        }

        public string DisplayFormat { get; set; }

        protected virtual bool OnValueChanged(TValue value)
        {
            return true;
        }
    }
}
