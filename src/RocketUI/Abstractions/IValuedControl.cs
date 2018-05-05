using System;

namespace RocketUI
{
    public interface IValuedControl<TValue> : IGuiControl
    {
        event EventHandler<TValue> ValueChanged;

        TValue Value { get; set; }

        string DisplayFormat { get; set; }

    }
}
