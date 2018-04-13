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
	            HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Top,

                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top,
				Spacing = 1
            });
            
            AddChild(_rightContainer = new GuiStackContainer()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment   = VerticalAlignment.Top,

                Orientation                = Orientation.Vertical,
	            HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment   = VerticalAlignment.Top,
	            Spacing = 1
			});
        }

        public void AddDebugLeft(Func<string> getDebugString)
        {
            _leftContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString)
            {
                TextColor = TextColor.White,
				//HorizontalAlignment = HorizontalAlignment.Left,
				Scale = 0.5f
            });
        }

        public void AddDebugRight(Func<string> getDebugString)
        {
            _rightContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString)
            {
                TextColor = TextColor.White,
				//HorizontalAlignment = HorizontalAlignment.Right,
	            Scale = 0.5f
			});
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            base.OnDraw(args);
        }
    }
}
