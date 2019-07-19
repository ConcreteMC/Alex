using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiToggleButton : GuiButton, IValuedControl<bool>
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
		            ValueChanged?.Invoke(this, _value);
	            }
	            
	            TextElement.Text = string.Format(DisplayFormat, _value);
            }
        }

	    public string DisplayFormat { get; set; }

	    public GuiToggleButton() : base()
	    {

	    }

	    protected override void OnCursorPressed(Point cursorPosition)
	    {
		    Value = !_value;
	    }
    }
}
