using Alex.Utils;

namespace Alex.Common.Commands.Parsers
{
	public class BoolArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public BoolArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader input)
		{
			if (input.ReadSingleWord(out var textInput) > 0 && bool.TryParse(textInput, out bool value))
				return true;

			return false;
		}
	}
}