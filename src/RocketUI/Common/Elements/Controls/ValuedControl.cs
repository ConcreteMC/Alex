using System;

namespace RocketUI.Elements.Controls
{
    public class ValuedControl<TValue> : Control, IValuedControl<TValue> where TValue : IEquatable<TValue>
    {
        public event EventHandler<TValue> ValueChanged;

	    public ValuedControl()
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
