using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui
{
    public static class GuiStyleHelper
    {
        public static TButton ApplyModernStyle<TButton>(this TButton button, bool modern = true)
            where TButton : Button
        {
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
                button.Background = GuiTextures.ButtonDefault;
                button.DisabledBackground = GuiTextures.ButtonDisabled;
                button.HighlightedBackground = GuiTextures.ButtonHover;
                button.FocusedBackground = GuiTextures.ButtonFocused;
                button.HighlightColor = (Color) TextColor.Yellow;
                button.DefaultColor = (Color) TextColor.White;
                
                if (button is ToggleButton toggleButton)
                {
                    toggleButton.CheckedColor = (Color) TextColor.Yellow;
                }
            }

            return button;
        }
    }
}