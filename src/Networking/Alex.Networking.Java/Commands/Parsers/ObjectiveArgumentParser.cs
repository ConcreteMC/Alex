using Alex.Interfaces;

namespace Alex.Networking.Java.Commands.Parsers
{
	public class ObjectiveArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public ObjectiveArgumentParser(string name) : base(name) { }

		/// <inheritdoc />
		public override bool TryParse(ISeekableTextReader input)
		{
			return false;
		}
	}
}