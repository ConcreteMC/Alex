using System;
using System.Linq;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;
using FontStyle = Alex.API.Graphics.Typography.FontStyle;

namespace Alex.Gui.Elements
{
    public class GuiDebugInfo : Screen
    {
        private Container _leftContainer, _rightContainer;

        public GuiDebugInfo() : base()
        {
            AddChild(_leftContainer = new StackContainer()
            {
                Orientation = Orientation.Vertical,

                Anchor = Alignment.TopLeft,
                ChildAnchor = Alignment.TopLeft,
            });
            
            AddChild(_rightContainer = new StackContainer()
            {
                Orientation = Orientation.Vertical,

                Anchor = Alignment.TopRight,
                ChildAnchor = Alignment.TopRight,
			});
        }

        public void AddDebugLeft(string text, bool hasBackground = true)
        {
            _leftContainer.AddChild(new TextElement(text, hasBackground)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f,
                TextAlignment = TextAlignment.Left
            });
        }

        public void AddDebugLeft(Func<string> getDebugString, TimeSpan interval = new TimeSpan(), bool hasBackground = true)
        {
            _leftContainer.AddChild(new AutoUpdatingTextElement(getDebugString, hasBackground)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f,
                Interval = interval,
                TextAlignment = TextAlignment.Left
            });
        }

        public void AddDebugRight(string text, bool hasBackground = true)
        {
            _rightContainer.AddChild(new TextElement(text, hasBackground)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f,
                TextAlignment = TextAlignment.Right
            });
        }

        public void AddDebugRight(Func<string> getDebugString, TimeSpan interval = new TimeSpan(), bool hasBackground = true)
        {
            _rightContainer.AddChild(new AutoUpdatingTextElement(getDebugString, hasBackground)
            {
                TextColor = TextColor.White,
                FontStyle = FontStyle.DropShadow,
                Scale = 1f,
                BackgroundOverlay = Color.Black * 0.25f,
                Interval = interval,
                TextAlignment = TextAlignment.Right
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
