using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RocketUI.IO.Serialization;

namespace RocketUI.Styling
{
    public class StyleCollection : Dictionary<Type, Dictionary<string, Style>>
    {

        public StyleCollection(IEnumerable<Style> styles)
        {
            BuildStyles(styles);
        }

        public StyleSheet GetElementStyles<TElement>(TElement element) where TElement : IStyledElement
        {
            return null;
        }

        private void BuildStyles(IEnumerable<Style> styles)
        {
            // Resolve TargetTypes
            

            // Build tree using "BasedOn"


        }


        private void AppendStyle(ref StyleSheet styles, Style style)
        {

        }
    }
}
