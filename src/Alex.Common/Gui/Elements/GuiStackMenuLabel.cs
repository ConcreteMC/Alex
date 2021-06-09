using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Common.Gui.Elements
{
    public class GuiStackMenuLabel : RocketElement
    {
        private TextElement TextElement { get; }

        public GuiStackMenuLabel(string text, bool isTranslationKey = false)
        {
            MinHeight = 20;
            MinWidth = 20;

            //MaxHeight = 22;
            //MaxWidth = 200;
            Padding = new Thickness(5, 5);
            Margin = new Thickness(2);

            AddChild(TextElement = new TextElement()
            {
                Margin = new Thickness(5),
                Anchor = Alignment.MiddleCenter,
                Text = text,
                TextColor = Color.White,
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