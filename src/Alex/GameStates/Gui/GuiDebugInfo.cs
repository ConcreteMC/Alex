using System;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
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
            });
            
            AddChild(_rightContainer = new GuiStackContainer()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment   = VerticalAlignment.Top,

                Orientation                = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment   = VerticalAlignment.Top,
            });
        }

        public void AddDebugLeft(Func<string> getDebugString)
        {
            _leftContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString));
        }

        public void AddDebugRight(Func<string> getDebugString)
        {
            _rightContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString));
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
