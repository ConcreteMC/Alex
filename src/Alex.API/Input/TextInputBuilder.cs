using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API.Input
{
    public class TextInputBuilder
    {
        public event EventHandler CursorPositionChanged; 

        public string Text { get; private set; }

        public int Length
        {
            get { return Text.Length; }
        }

        private int _cursorPosition;

        public int CursorPosition
        {
            get
            {
                return _cursorPosition;
            }
            set
            {
                var p = MathHelper.Clamp(value, 0, Length);

                if (p != _cursorPosition)
                {
                    _cursorPosition = p;
                    CursorPositionChanged?.Invoke(this, null);
                }
            }
        }

        public TextInputBuilder(string text = null)
        {
            Text = text ?? string.Empty;
        }

        public void RemoveCharacter()
        {
            var t   = Text;
            var pos = CursorPosition;
            
            Text           = t.Substring(0, pos - 1) + t.Substring(pos, t.Length-pos);
            CursorPosition = pos + 1;
        }

        public void AppendCharacter(char c)
        {
            var t = Text;
            var pos = CursorPosition;

            var str = c.ToString();
            
            Text = t.Substring(0, pos) + str + t.Substring(pos, t.Length-pos);
            CursorPosition = pos + 1;
        }
    }
}
