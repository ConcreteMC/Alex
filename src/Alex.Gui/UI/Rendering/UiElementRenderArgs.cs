using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.UI.Layout;
using Alex.Graphics.UI.Themes;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Rendering
{
    public class UiElementRenderArgs
    {

        public UiElement Element { get; }

        public Rectangle LayoutBounds { get; }
        public Rectangle Bounds { get; }
        public Rectangle ContentBounds { get; }

        public Vector2 Position { get; }

        public UiElementStyle Style { get; }
        public UiElementLayoutParameters Layout { get; }

        public UiElementRenderArgs(UiElement element)
        {
            Element = element;

            Bounds = Element.LayoutParameters.Bounds;
            ContentBounds = Element.LayoutParameters.InnerBounds;
            LayoutBounds = Element.LayoutParameters.OuterBounds;

            Position = ContentBounds.Location.ToVector2();

            Style = Element.Style;
            Layout = Element.LayoutParameters;
        }

    }
}
