using System;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

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
                if (!value.Equals(_value))
                {
                    _value = value;
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public string DisplayFormat { get; set; }
    }
}
