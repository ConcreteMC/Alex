using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API.Input
{
    public class TextInputBuilder
    {
        public event EventHandler CursorPositionChanged;

	    private StringBuilder _stringBuilder;
	    public string Text => _stringBuilder.ToString();
	    public int Length => _stringBuilder.Length;

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
	        if (text != null)
	        {
				_stringBuilder = new StringBuilder(text);
			}
	        else
	        {
				_stringBuilder = new StringBuilder();
	        }
        }

        public void RemoveCharacter()
        {
	        if (_stringBuilder.Length == 0) return;

            var pos = CursorPosition;

	        _stringBuilder.Remove(pos - 1, 1);
            CursorPosition = pos - 1;
        }

        public void AppendCharacter(char c)
        {
            var pos = CursorPosition;
	        _stringBuilder.Insert(pos, c);

            CursorPosition = pos + 1;
        }

	    public void AppendLine(string line)
		{
			var pos = CursorPosition;
			_stringBuilder.Insert(pos, line);
			CursorPosition = pos + line.Length;
		}

	    public void Clear()
	    {
		    _stringBuilder.Clear();
			CursorPosition = 0;
	    }
    }
}
