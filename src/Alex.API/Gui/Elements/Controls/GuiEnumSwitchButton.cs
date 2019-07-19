using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiEnumSwitchButton<TEnum> : GuiButton, IValuedControl<TEnum> where TEnum : Enum
    {
        public event EventHandler<TEnum> ValueChanged;
        
        private TEnum[] Values { get; set; }
        private int CurrentIndex { get; set; } = 0;
        public string DisplayFormat { get; set; } = "{0}";

        public TEnum Value
        {
            get { return Values[CurrentIndex]; }
            set
            {
                for (int i = 0; i < Values.Length; i++)
                {
                    if (Values[i].Equals(value))
                    {
                        CurrentIndex = i;
                        Text = string.Format(DisplayFormat, value.ToString());
            
                        ValueChanged?.Invoke(this, value);
                        return;
                    }
                }
                
                
            }
        }

        public GuiEnumSwitchButton()
        {
            var values = Enum.GetValues(typeof(TEnum));

            List<TEnum> v = new List<TEnum>();
            foreach (var value in values)
            {
                v.Add((TEnum) value);
            }

            Values = v.ToArray();

            Text = string.Format(DisplayFormat, Values[CurrentIndex].ToString());
        }

        protected override void OnCursorPressed(Point cursorPosition)
        {
            CurrentIndex++;
            if (CurrentIndex >= Values.Length)
            {
                CurrentIndex = 0;
            }

            var value = Values[CurrentIndex];
            Text = string.Format(DisplayFormat, value.ToString());
            
            ValueChanged?.Invoke(this, value);
        }
    }
}