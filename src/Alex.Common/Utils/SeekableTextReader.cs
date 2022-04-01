using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.Interfaces;
using NLog;
using NLog.Fluent;

namespace Alex.Utils
{
	public class SeekableTextReader : TextReader, ISeekableTextReader
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SeekableTextReader));

		private string _text;
		private int _position = 0;

		public SeekableTextReader(string text) : base()
		{
			_text = text;
		}

		/// <inheritdoc />
		public override int Read()
		{
			if (_position == _text.Length)
				return -1;

			char c = _text[_position];
			_position++;

			return c;
		}

		/// <inheritdoc />
		public override int Peek()
		{
			if (_position == _text.Length)
				return -1;

			char c = _text[_position];

			return c;
		}

		public int Position
		{
			get => _position;
			set
			{
				if (value >= _text.Length || value < 0)
					throw new IndexOutOfRangeException();

				_position = value;
			}
		}

		public int Length => _text.Length;

		public int ReadUntil(char c, out string result)
		{
			StringBuilder sb = new StringBuilder();

			char readCharacter;
			int read = -1;

			do
			{
				read = Read();

				if (read == -1)
					break;

				readCharacter = (char)read;
				sb.Append(readCharacter);
			} while (readCharacter != c);

			result = sb.ToString();
			Log.Info($"ReadUntil: {sb.ToString()}");

			return sb.Length;
		}

		public int ReadSingleWord(out string result)
		{
			return ReadUntil(' ', out result);
		}

		public int ReadQuoted(out string result)
		{
			result = string.Empty;

			if (Peek() != '"')
				return -1;

			Read();

			return ReadUntil('"', out result);
		}
	}
}