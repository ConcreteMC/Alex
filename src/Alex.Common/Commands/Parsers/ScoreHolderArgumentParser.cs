using Alex.Utils;

namespace Alex.Common.Commands.Parsers
{
	public class ScoreHolderArgumentParser : ArgumentParser
	{
		private byte _flags;

		/// <inheritdoc />
		public ScoreHolderArgumentParser(string name, byte flags) : base(name)
		{
			_flags = flags;
		}

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader input)
		{
			return false;
		}
	}

	public class EntityArgumentParser : ArgumentParser
	{
		private byte _flags;

		/// <inheritdoc />
		public EntityArgumentParser(string name, byte flags) : base(name)
		{
			_flags = flags;
		}

		/// <inheritdoc />
		public override bool TryParse(SeekableTextReader input)
		{
			return false;
		}
	}
}