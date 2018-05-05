using System;

namespace RocketUI.Elements.Controls
{
    public class ToggleButton : Button, IValuedControl<bool>
    {

	    public event EventHandler<bool> ValueChanged;
	    private bool _value;

	    public bool Value
        {
            get => _value;
            set
            {
	            if (value != _value)
	            {
		            _value = value;
		            TextBlock.Text = string.Format(DisplayFormat, _value);
		            ValueChanged?.Invoke(this, _value);
	            }
            }
        }

	    public string DisplayFormat { get; set; }

	    public ToggleButton() : base()
	    {

	    }
		
    }
}
