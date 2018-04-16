using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Gui
{
    public interface IValuedControl<TValue>
    {
        event EventHandler<TValue> ValueChanged;

        TValue Value { get; set; }

        string DisplayFormat { get; set; }

    }
}
