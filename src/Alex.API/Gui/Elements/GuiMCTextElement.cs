using Alex.API.Gui.Elements.Controls;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
{
    public class GuiMCTextElement : GuiTextElement
    {
        private TextColor _textColor = TextColor.White;
        public TextColor TextColor
        {
            get
            {
                if (_textColor == null)
                {
                    _textColor = new TextColor(base.Foreground, base.ForegroundShadow);
                }

                return _textColor;
            }
            set
            {
                _textColor = value;

                base.Foreground = value?.ForegroundColor ?? Color.White;
                base.ForegroundShadow = value?.BackgroundColor ?? Color.Transparent;
            }
        }

    }
}
