using System;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiButton : GuiControl
    {

        public string Text
        {
            get => TextElement.Text;
	        set => TextElement.Text = value;
        }

        protected GuiTextElement TextElement { get; }
        protected Action Action { get; }
		
	    public GuiButton() : this(string.Empty)
	    {

	    }
		
        public GuiButton(string text, Action action = null)
        {
            DefaultBackgroundTexture = GuiTextures.ButtonDefault;
            HighlightedBackgroundTexture = GuiTextures.ButtonHover;
            FocusedBackgroundTexture = GuiTextures.ButtonFocused;
            BackgroundRepeatMode = TextureRepeatMode.NoScaleCenterSlice;
			
            Action = action;
            MinHeight = 20;
	        MinWidth = 20;
			Padding = new Thickness(5, 5);
			Margin = new Thickness(2);

            TextElement = new GuiTextElement()
            {
				Margin = Thickness.Zero,
                Anchor = Alignment.MiddleCenter,
                Text = text,
                TextColor = TextColor.White,
				Opacity = 0.875f
            };
            AddChild(TextElement);
        }


	    protected override void OnClick(Vector2 relativePosition)
        {
            Action?.Invoke();
        }
    }
}
