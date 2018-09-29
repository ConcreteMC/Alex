using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API.Input
{
    public class TextInputBuilder
    {
	    public event EventHandler<string> TextChanged;
        public event EventHandler CursorPositionChanged;

	    private StringBuilder _stringBuilder;
	    public string Text 
	    {
		    get { return _stringBuilder.ToString(); }
		    set
		    {
			    var text = value;
			    if (!string.IsNullOrEmpty(text))
			    {
				    _stringBuilder  = new StringBuilder(text);
				    CursorPosition  = Length;

					TextChanged?.Invoke(this, Text);
			    }
			    else
			    {
				    _stringBuilder = new StringBuilder();
				    _cursorPosition = 0;

				    TextChanged?.Invoke(this, string.Empty);
			    }
		    }
	    }
		
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
	        Text = text;
        }

        public void RemoveCharacter()
        {
	        if (_stringBuilder.Length == 0) return;

            var pos = CursorPosition;
	        if (pos == 0) return;

	        _stringBuilder.Remove(pos - 1, 1);

	        TextChanged?.Invoke(this, Text);
            CursorPosition = pos - 1;
        }

        public void AppendCharacter(char c)
        {
            var pos = CursorPosition;
	        _stringBuilder.Insert(pos, c);
			
	        TextChanged?.Invoke(this, Text);
            CursorPosition = pos + 1;
        }

	    public void AppendLine(string line)
		{
			var pos = CursorPosition;
			_stringBuilder.Insert(pos, line);

			TextChanged?.Invoke(this, Text);
			CursorPosition = pos + line.Length;
		}

	    public string GetAllBehindCursor(out int cursorPos)
	    {
		    if (Length == 0)
		    {
			    CursorPosition = cursorPos = 0;
			    return String.Empty;
		    }

		    var pos = CursorPosition;
		    while (pos > Length)
		    {
			    pos--;
		    }

		    cursorPos = pos;

			char[] dest = new char[Length - pos];
		    _stringBuilder.CopyTo(pos, dest, 0, dest.Length);

			return new string(dest);
	    }

	    public void Clear()
	    {
		    _stringBuilder.Clear();
			
		    TextChanged?.Invoke(this, string.Empty);
			CursorPosition = 0;
	    }
    }
}
