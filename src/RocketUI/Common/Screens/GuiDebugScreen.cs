using System;
using RocketUI.Elements;
using RocketUI.Elements.Layout;

namespace RocketUI.Screens
{
    public class GuiDebugScreen : GuiScreen
    {

        public GuiStackContainer TopLeft { get; }
        public GuiStackContainer TopRight { get; }
        public GuiStackContainer BottomLeft  { get; }
        public GuiStackContainer BottomRight { get; }

        public GuiDebugScreen()
        {
            AddChild(TopLeft     = new GuiStackContainer
            {
                Anchor = Anchor.TopLeft,
                ChildAnchor = Anchor.TopLeft,
            });
            AddChild(TopRight    = new GuiStackContainer
            {
                Anchor = Anchor.TopRight,
                ChildAnchor = Anchor.TopRight,
            });
            AddChild(BottomLeft  = new GuiStackContainer
            {
                Anchor = Anchor.BottomLeft,
                ChildAnchor = Anchor.BottomLeft,
            });
            AddChild(BottomRight = new GuiStackContainer
            {
                Anchor = Anchor.BottomRight,
                ChildAnchor = Anchor.BottomRight,
            });
        }

        public void AddTopLeft(Func<string> autoTextFunc)
        {
            TopLeft.AddChild(new GuiAutoUpdatingTextElement(autoTextFunc) { FontFamily = "Debug" });
        }
        public void AddTopRight(Func<string> autoTextFunc)
        {
            TopRight.AddChild(new GuiAutoUpdatingTextElement(autoTextFunc) { FontFamily = "Debug" });
        }
        public void AddBottomLeft(Func<string> autoTextFunc)
        {
            BottomLeft.AddChild(new GuiAutoUpdatingTextElement(autoTextFunc) { FontFamily = "Debug" });
        }
        public void AddBottomRight(Func<string> autoTextFunc)
        {
            BottomRight.AddChild(new GuiAutoUpdatingTextElement(autoTextFunc) { FontFamily = "Debug" });
        }

    }
}
