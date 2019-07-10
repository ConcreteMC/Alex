using System;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using RocketUI;

namespace Alex.Gui.Elements
{
    public class GuiDebugInfo : GuiScreen
    {
        private GuiContainer _leftContainer, _rightContainer;

        public GuiDebugInfo() : base()
        {
            AddChild(_leftContainer = new GuiStackContainer()
            {
                Orientation = Orientation.Vertical,

                Anchor = Alignment.TopLeft,
                ChildAnchor = Alignment.TopLeft,
            });
            
            AddChild(_rightContainer = new GuiStackContainer()
            {
                Orientation = Orientation.Vertical,

                Anchor = Alignment.TopRight,
                ChildAnchor = Alignment.TopRight,
			});
        }

        public void AddDebugLeft(Func<string> getDebugString)
        {
            _leftContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString, true)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 0.5f,
            });
        }

        public void AddDebugRight(Func<string> getDebugString)
        {
            _rightContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString, true)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 0.5f,
			});
        }
    }
}
