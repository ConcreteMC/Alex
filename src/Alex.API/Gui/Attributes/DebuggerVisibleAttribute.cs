using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Gui.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DebuggerVisibleAttribute : Attribute
    {

        public bool Visible { get; set; } = true;

        public DebuggerVisibleAttribute()
        {

        }


    }
}
