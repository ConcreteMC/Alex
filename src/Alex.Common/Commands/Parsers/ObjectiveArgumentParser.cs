using Alex.Utils;

namespace Alex.Common.Commands.Parsers
{
	public class ObjectiveArgumentParser : ArgumentParser
	{
		/// <inheritdoc />
		public ObjectiveArgumentParser(string name) : base(name)
		{
			
		}

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader input)
		{
			return false;
		}
	}
}