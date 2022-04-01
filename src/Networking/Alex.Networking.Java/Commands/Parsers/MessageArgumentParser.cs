using Alex.Interfaces;

namespace Alex.Networking.Java.Commands.Parsers
{
	public class MessageArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public MessageArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			return false;
		}
	}
}