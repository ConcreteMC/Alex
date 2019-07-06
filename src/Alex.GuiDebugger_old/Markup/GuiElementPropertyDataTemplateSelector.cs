using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Alex.GuiDebugger.Markup
{
    public class GuiElementPropertyDataTemplateSelector : DataTemplateSelector
    {

        public DataTemplate StringTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }
        public DataTemplate IntTemplate { get; set; }
        public DataTemplate PointTemplate { get; set; }
        public DataTemplate Vector2Template { get; set; }


    }
}
