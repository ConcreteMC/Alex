using System;
using Alex.Interfaces;
using Alex.Networking.Java.Commands.Parsers;

namespace Alex.Utils.Commands
{
	public class TextCommandProperty : CommandProperty
	{
		public StringArgumentParser.StringMode Mode { get; set; } = StringArgumentParser.StringMode.SingleWord;

		/// <inheritdoc />
		public TextCommandProperty(string name, bool required = true) : base(name, required, "text") { }


		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader reader)
		{
			string textInput = null;
			int length;

			switch (Mode)
			{
				case StringArgumentParser.StringMode.SingleWord:
					length = reader.ReadSingleWord(out textInput);

					break;

				case StringArgumentParser.StringMode.QuotablePhrase:
					length = reader.ReadQuoted(out textInput);

					break;

				case StringArgumentParser.StringMode.GreedyPhrase:
					textInput = reader.ReadToEnd();
					length = textInput.Length;

					break;

				default:
					throw new ArgumentOutOfRangeException();
			}


			return length >= 0;
		}
	}
}