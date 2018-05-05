using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI.Elements;

namespace Alex.API.Gui.Elements
{
    public class GuiAutoUpdatingMCTextElement : GuiAutoUpdatingTextElement
    {
        private TextColor _textColor;
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

                base.Foreground       = value?.ForegroundColor ?? Color.White;
                base.ForegroundShadow = value?.BackgroundColor ?? Color.Transparent;
            }
        }

        public GuiAutoUpdatingMCTextElement(Func<string> updateFunc, bool hasBackground = false) : base(updateFunc, hasBackground)
        {
        }
    }
}
