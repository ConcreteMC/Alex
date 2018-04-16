using System;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiBeaconButton : GuiControl
    {

        public string Text => TextElement.Text;

        protected GuiTextElement TextElement { get; }
        protected Action Action { get; }

	    public GuiBeaconButton() : this(string.Empty)
	    {

	    }
		
        public GuiBeaconButton(string text, Action action = null)
        {
            DefaultBackgroundTexture = GuiTextures.ButtonDefault;
            HighlightedBackgroundTexture = GuiTextures.ButtonHover;
            FocusedBackgroundTexture = GuiTextures.ButtonFocused;
            BackgroundRepeatMode = TextureRepeatMode.NoScaleCenterSlice;

            Action = action;
            Height = 20;

            TextElement = new GuiTextElement()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = text,
                TextColor = TextColor.White
            };
            AddChild(TextElement);
        }

	    protected override void OnUpdateLayout()
	    {
		    int minWidth = TextElement.Width + 20;
		    if (Width <= minWidth)
		    {
			    Width = minWidth;
		    }

			base.OnUpdateLayout();
	    }


	    protected override void OnClick(Vector2 relativePosition)
        {
            Action?.Invoke();
        }
    }
}
