using System;
using System.Collections.Generic;
using System.Text;
using RocketUI.Styling;

namespace RocketUI
{
    public interface IStyledElement : IVisualElement
    {
        StyledElementPropertyCollection StyledProperties { get; }

        string ClassName { get; set; }

        StyleSheet Styles { get; set; }

        void InvalidateStyle(IStyledElement sender);

    }
}
