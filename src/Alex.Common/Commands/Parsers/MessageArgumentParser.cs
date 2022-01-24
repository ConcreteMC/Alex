using Alex.Utils;

namespace Alex.Common.Commands.Parsers
{
	public class MessageArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public MessageArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader input)
		{
			return false;
		}
	}
}