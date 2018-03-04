using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Gui.Themes
{
    public class UiThemeStyleSheet
    {

        public UiElementPredicate Predicate { get; }
        public UiElementStyle Style { get; }

        public UiThemeStyleSheet(UiElementPredicate predicate, UiElementStyle style)
        {
            Predicate = predicate;
            Style = style;
        }

    }
}
