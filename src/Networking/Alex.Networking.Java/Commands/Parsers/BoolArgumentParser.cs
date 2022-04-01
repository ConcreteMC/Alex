using Alex.Interfaces;

namespace Alex.Networking.Java.Commands.Parsers
{
	public class BoolArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public BoolArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			if (input.ReadSingleWord(out var textInput) > 0 && bool.TryParse(textInput, out bool value))
				return true;

			return false;
		}
	}
}