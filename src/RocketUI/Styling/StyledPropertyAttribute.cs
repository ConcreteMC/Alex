using System;
using System.Collections.Generic;
using System.Text;

namespace RocketUI.Styling
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StyledPropertyAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public StyledPropertyAttribute(string propertyName = null)
        {
            PropertyName = propertyName;
        }

    }
}
