using System;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui
{
	public static class GuiStyleHelper
	{
		public static TSlider ApplyStyle<TSlider, TValue>(this TSlider slider)
			where TSlider : Slider<TValue> where TValue : IConvertible
		{
			return slider;

			slider.Background = AlexGuiTextures.ButtonDisabled;
			slider.ThumbBackground = AlexGuiTextures.ButtonDefault;
			slider.ThumbHighlightBackground = AlexGuiTextures.ButtonHover;
			slider.ThumbDisabledBackground = AlexGuiTextures.ButtonDisabled;

			slider.Background.RepeatMode = TextureRepeatMode.Stretch;
			slider.ThumbBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
			slider.ThumbHighlightBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
			slider.ThumbDisabledBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

			/*
			if (modern)
			{
			    button.Background = Color.Transparent;
			    button.DisabledBackground = Color.Transparent;
			    button.FocusedBackground = Color.TransparentBlack;
			    button.HighlightedBackground = new Color(Color.Black * 0.8f, 0.5f);
			    button.HighlightColor = (Color) TextColor.Cyan;
			    button.DefaultColor = (Color) TextColor.White;
			    
			    if (button is ToggleButton toggleButton)
			    {
			        toggleButton.CheckedColor = (Color) TextColor.Cyan;
			    }
			}
			else
			{
			    button.Background = AlexGuiTextures.ButtonDefault;
			    button.DisabledBackground = AlexGuiTextures.ButtonDisabled;
			    button.HighlightedBackground = AlexGuiTextures.ButtonHover;
			    button.FocusedBackground = AlexGuiTextures.ButtonFocused;
			    button.HighlightColor = (Color) TextColor.Yellow;
			    button.DefaultColor = (Color) TextColor.White;
			    
			    if (button is ToggleButton toggleButton)
			    {
			        toggleButton.CheckedColor = (Color) TextColor.Yellow;
			    }
			}*/

			return slider;
		}

		public static TSlider ApplyStyle<TSlider>(this TSlider slider) where TSlider : Slider
		{
			return slider;

			slider.Background = AlexGuiTextures.ButtonDisabled;
			slider.ThumbBackground = AlexGuiTextures.ButtonDefault;
			slider.ThumbHighlightBackground = AlexGuiTextures.ButtonHover;
			slider.ThumbDisabledBackground = AlexGuiTextures.ButtonDisabled;

			slider.Background.RepeatMode = TextureRepeatMode.Stretch;
			slider.ThumbBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
			slider.ThumbHighlightBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
			slider.ThumbDisabledBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

			/*
			if (modern)
			{
			    button.Background = Color.Transparent;
			    button.DisabledBackground = Color.Transparent;
			    button.FocusedBackground = Color.TransparentBlack;
			    button.HighlightedBackground = new Color(Color.Black * 0.8f, 0.5f);
			    button.HighlightColor = (Color) TextColor.Cyan;
			    button.DefaultColor = (Color) TextColor.White;
			    
			    if (button is ToggleButton toggleButton)
			    {
			        toggleButton.CheckedColor = (Color) TextColor.Cyan;
			    }
			}
			else
			{
			    button.Background = AlexGuiTextures.ButtonDefault;
			    button.DisabledBackground = AlexGuiTextures.ButtonDisabled;
			    button.HighlightedBackground = AlexGuiTextures.ButtonHover;
			    button.FocusedBackground = AlexGuiTextures.ButtonFocused;
			    button.HighlightColor = (Color) TextColor.Yellow;
			    button.DefaultColor = (Color) TextColor.White;
			    
			    if (button is ToggleButton toggleButton)
			    {
			        toggleButton.CheckedColor = (Color) TextColor.Yellow;
			    }
			}*/

			return slider;
		}

		public static TButton ApplyModernStyle<TButton>(this TButton button, bool modern = true) where TButton : Button
		{
			if (modern)
			{
				if (!button.ClassNames.Contains("Modern"))
					button.ClassNames.Add("Modern");
			}
			else
			{
				if (button.ClassNames.Contains("Modern"))
					button.ClassNames.Remove("Modern");
			}

			return button;
		}
	}
}