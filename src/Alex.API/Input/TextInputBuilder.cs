using System;
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

		public bool IsSelecting
		{
			get => _isSelecting;
			set
			{
				if (_isSelecting == value) return;

				if (!_isSelecting && value)
				{
					_selectionStartPosition = CursorPosition;
				}
				
				_isSelecting = value;
			}
		}

		public int Length => _stringBuilder.Length;

		public bool HasSelection => _selectionStartPosition > -1;

		public int SelectionStartPosition => HasSelection ? Math.Min(_cursorPosition, _selectionStartPosition) : -1;

		public int SelectionEndPosition => HasSelection ? Math.Max(_cursorPosition, _selectionStartPosition) : -1;

		public string SelectedText => HasSelection ? 
			Text.Substring(SelectionStartPosition, SelectionEndPosition - SelectionStartPosition) : string.Empty;


		private int _cursorPosition;
		private int _selectionStartPosition = -1;
		private bool _isSelecting;

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

			if (IsSelecting)
			{
				RemoveSelection();
				return;
			}

            var pos = CursorPosition;
	        if (pos == 0) return;

	        _stringBuilder.Remove(pos - 1, 1);

	        TextChanged?.Invoke(this, Text);
            CursorPosition = pos - 1;
        }

        public void AppendCharacter(char c)
        {
			if (IsSelecting)
			{
				RemoveSelection();
			}

            var pos = CursorPosition;
	        _stringBuilder.Insert(pos, c);
			
	        TextChanged?.Invoke(this, Text);
            CursorPosition = pos + 1;
        }

		public void Append(string str)
		{
			if (IsSelecting)
			{
				RemoveSelection();
			}

			var pos = CursorPosition;
			_stringBuilder.Insert(pos, str);

			TextChanged?.Invoke(this, Text);
			CursorPosition = pos + str.Length;
		}

		public void RemoveSelection()
		{
			if (_stringBuilder.Length == 0 || !HasSelection) return;

			var pos = CursorPosition;

			var start = SelectionStartPosition;
			_stringBuilder.Remove(start, SelectionEndPosition-start);
			_selectionStartPosition = -1;
			IsSelecting = false;

			TextChanged?.Invoke(this, Text);
			_cursorPosition = start;
		}

		public void SelectLeft(bool selectWord = false)
		{
			if (_selectionStartPosition < 0)
			{
				IsSelecting = true;
				_selectionStartPosition = CursorPosition;
			}
			CursorPosition--;
		}

		public void SelectRight(bool selectWord = false)
		{
			if (_selectionStartPosition < 0)
			{
				IsSelecting             = true;
				_selectionStartPosition = CursorPosition;
			}
			CursorPosition++;
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
			_selectionStartPosition = -1;
			IsSelecting = false;
		}

		public void ClearSelection()
		{
			_selectionStartPosition = -1;
			IsSelecting = false;
		}

		public void SelectAll()
		{
			IsSelecting = true;
			_selectionStartPosition = 0;
			CursorPosition = Length;
		}
    }
}
