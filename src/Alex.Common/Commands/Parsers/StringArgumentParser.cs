using System;
using Alex.Utils;

namespace Alex.Common.Commands.Parsers
{
	public class StringArgumentParser : ArgumentParser
	{
		public StringMode Mode { get; }
		/// <inheritdoc />
		public StringArgumentParser(string name, StringMode mode) : base(name)
		{
			Mode = mode;
		}

		public enum StringMode
		{
			SingleWord,
			QuotablePhrase,
			GreedyPhrase
		}

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader input)
		{
			string textInput = null;
			int length;
			switch (Mode)
			{
				case StringMode.SingleWord:
					length = input.ReadSingleWord(out textInput);
					break;

				case StringMode.QuotablePhrase:
					length = input.ReadQuoted(out textInput);
					break;

				case StringMode.GreedyPhrase:
					textInput = input.ReadToEnd();
					length = textInput.Length;
					
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}


			return length >= 0;
		}
	}

	public class ResourceLocationArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public ResourceLocationArgumentParser(string name) : base(name)
		{
			
		}

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader input)
		{
			if (input.ReadSingleWord(out var textInput) > 0)
			{
				return true;
			}

			return false;
		}
	}
}