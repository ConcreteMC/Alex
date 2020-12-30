using System;
using Alex.API.Graphics.Typography;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiStackMenuItem : GuiButton
    {
        public GuiStackMenuItem(string text, Action action, bool isTranslationKey = false) : base(text, action, isTranslationKey)
        {
        }
        
        public GuiStackMenuItem(){}
    }

    public class GuiStackMenuLabel : GuiElement
    {
        private GuiTextElement TextElement { get; }
        public GuiStackMenuLabel(string text, bool isTranslationKey = false)
        {
            MinHeight = 20;
            MinWidth = 20;

            //MaxHeight = 22;
            //MaxWidth = 200;
            Padding = new Thickness(5, 5);
            Margin = new Thickness(2);

            AddChild(TextElement = new GuiTextElement()
            {
                Margin =  new Thickness(5),
                Anchor = Alignment.MiddleCenter,
                Text = text,
                TextColor = TextColor.White,
                TextOpacity = 0.875f,
                FontStyle = FontStyle.DropShadow,
                //Enabled = false,
                //CanFocus = false
            });
			
            if (isTranslationKey)
            {
                TextElement.TranslationKey = text;
            }
            
            Background = Color.Transparent;
        }
    }
}
