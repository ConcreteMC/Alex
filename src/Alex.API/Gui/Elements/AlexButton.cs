using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.API.Gui.Elements
{
	public class AlexButton : Button
	{
		public AlexButton(Action action = null) : this(string.Empty, action)
		{
		}

		public AlexButton(string text, Action action = null, bool isTranslationKey = false) : base(text, action, isTranslationKey)
		{
			IsModern = true;
		}

		private bool _isModern = false;
		public bool IsModern
		{
			get
			{
				return _isModern;
			}
			set
			{
				_isModern = value;
				if (value)
				{
					Background = Color.Transparent;
					DisabledBackground = Color.Transparent;
					FocusedBackground = Color.Transparent;
					HighlightedBackground = new Color(Color.Black * 0.8f, 0.5f);
					HighlightColor = (Color) TextColor.Cyan;
					DefaultColor = (Color) TextColor.White;
                
					/*if (button is ToggleButton toggleButton)
					{
						toggleButton.CheckedColor = (Color) TextColor.Cyan;
					}*/
				}
				else
				{
					Background = GuiTextures.ButtonDefault;
					DisabledBackground = GuiTextures.ButtonDisabled;
					HighlightedBackground = GuiTextures.ButtonHover;
					FocusedBackground = GuiTextures.ButtonFocused;
					HighlightColor = (Color) TextColor.Yellow;
					DefaultColor = (Color) TextColor.White;
                
					/*if (button is ToggleButton toggleButton)
					{
						toggleButton.CheckedColor = (Color) TextColor.Yellow;
					}*/
				}
			}
		}
	}
}