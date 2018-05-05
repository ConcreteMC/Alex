using System;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Elements.Layout;
using RocketUI.Screens;

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

                Anchor = Anchor.TopLeft,
                ChildAnchor = Anchor.TopLeft,
            });
            
            AddChild(_rightContainer = new GuiStackContainer()
            {
                Orientation = Orientation.Vertical,

                Anchor = Anchor.TopRight,
                ChildAnchor = Anchor.TopRight,
			});
        }

        public void AddDebugLeft(Func<string> getDebugString)
        {
            _leftContainer.AddChild(new GuiAutoUpdatingMCTextElement(getDebugString, true)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 0.5f,
            });
        }

        public void AddDebugRight(Func<string> getDebugString)
        {
            _rightContainer.AddChild(new GuiAutoUpdatingMCTextElement(getDebugString, true)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 0.5f,
			});
        }
    }
}
