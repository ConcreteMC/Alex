using System;
using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
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
            _leftContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString, false)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f
            });
        }

        public void AddDebugRight(Func<string> getDebugString)
        {
            _rightContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString, false)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f
			});
        }

        /// <inheritdoc />
        protected override void OnUpdate(GameTime gameTime)
        {
            /*var scale = 2f / GuiRenderer.ScaledResolution.ScaleFactor;
            foreach (var child in _rightContainer.ChildElements.Where(x => x is GuiAutoUpdatingTextElement).Cast<GuiAutoUpdatingTextElement>())
            {
                child.Scale = scale;
            }
            foreach (var child in _leftContainer.ChildElements.Where(x => x is GuiAutoUpdatingTextElement).Cast<GuiAutoUpdatingTextElement>())
            {
                child.Scale = scale;
            }*/
            
            base.OnUpdate(gameTime);
        }
    }
}
