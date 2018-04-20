using System;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Gui
{
    public class GuiDebugInfo : GuiScreen
    {
        private GuiContainer _leftContainer, _rightContainer;

        public GuiDebugInfo(Game game) : base(game)
        {
            AddChild(_leftContainer = new GuiStackContainer()
            {
                Anchor = Alignment.TopLeft,

                Orientation = Orientation.Vertical,
                ChildAnchor = Alignment.TopLeft,
                //HorizontalContentAlignment = HorizontalAlignment.Left,
                //VerticalContentAlignment = VerticalAlignment.Top,
            });
            
            AddChild(_rightContainer = new GuiStackContainer()
            {
                Anchor = Alignment.TopRight,

                Orientation                = Orientation.Vertical,
                ChildAnchor = Alignment.TopRight,
	           // HorizontalContentAlignment = HorizontalAlignment.Right,
                //VerticalContentAlignment   = VerticalAlignment.Top,
			});
        }

        public void AddDebugLeft(Func<string> getDebugString)
        {
            _leftContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString)
            {
                TextColor = TextColor.White,
				//HorizontalAlignment = HorizontalAlignment.MinX,
				Scale = 0.5f
            });
        }

        public void AddDebugRight(Func<string> getDebugString)
        {
            _rightContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString)
            {
                TextColor = TextColor.White,
				//HorizontalAlignment = HorizontalAlignment.MaxX,
	            Scale = 0.5f
			});
        }
    }
}
