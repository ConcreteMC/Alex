using System;
using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

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

        public void AddDebugLeft(string text)
        {
            _leftContainer.AddChild(new GuiTextElement(text, false)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f
            });
        }

        public void AddDebugLeft(Func<string> getDebugString, TimeSpan interval = new TimeSpan())
        {
            _leftContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString, false)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f,
                Interval = interval
            });
        }

        public void AddDebugRight(string text)
        {
            _rightContainer.AddChild(new GuiTextElement(text, false)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f
            });
        }

        public void AddDebugRight(Func<string> getDebugString, TimeSpan interval = new TimeSpan())
        {
            _rightContainer.AddChild(new GuiAutoUpdatingTextElement(getDebugString, false)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f,
                Interval = interval
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
