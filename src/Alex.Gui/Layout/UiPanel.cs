using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Gui.Layout
{
    public class UiPanel : UiContainer
    {
        public override void UpdateSize()
        {
            ActualWidth = Width.HasValue ? Width.Value : (Container?.ClientBounds.Width ?? 0);
            ActualHeight = Height.HasValue ? Height.Value : (Container?.ClientBounds.Height ?? 0);
        }
    }
}
